using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Channels;
using GameMarketIntel.Collector.ExternalServices.Igdb;
using GameMarketIntel.Collector.ExternalServices.Igdb.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Shouldly;

namespace GameMarketIntel.Collector.Tests.ExternalServices.Igdb;

// Component integration tests: real DI and services, but no external network or database.
public sealed class IgdbIntegrationFlowTests
{
    private static readonly TimeSpan Watchdog = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan QueryDeadline = TimeSpan.FromSeconds(30);

    [Fact]
    public async Task ReadPagesAsync_ShouldCombineRetryRenewalPacingAndPagination()
    {
        // Arrange
        using var context = new TestContext();
        var query = new IgdbPageQuery("id", 2);
        context.RespondWith(HttpStatusCode.TooManyRequests, "remote-error", TimeSpan.FromSeconds(1));
        context.RespondWith(HttpStatusCode.Unauthorized, "remote-error");
        context.RespondWith(HttpStatusCode.OK, "[{\"id\":1},{\"id\":2}]");
        context.RespondWith(HttpStatusCode.OK, "[{\"id\":3}]");

        // Act
        var task = ReadPagesAsync(context.Paginator, query, context.Cancellation.Token);
        await context.Clock.AdvanceNextDelayAsync(TimeSpan.FromSeconds(1));
        await context.Clock.AdvanceNextDelayAsync(TimeSpan.FromMilliseconds(300));
        await context.Clock.AdvanceNextDelayAsync(TimeSpan.FromMilliseconds(300));
        var pages = await task.WaitAsync(Watchdog);

        // Assert
        pages.ShouldBe(new[] { "[{\"id\":1},{\"id\":2}]", "[{\"id\":3}]" });
        context.OAuthBodies.Count.ShouldBe(2);
        context.OAuthBodies.ShouldAllBe(body => body.Contains("grant_type=client_credentials"));
        context.OAuthBodies.ShouldAllBe(body => body.Contains("client_id=test-client"));
        context.OAuthBodies.ShouldAllBe(body => body.Contains("client_secret=test-secret"));
        context.Requests.Count.ShouldBe(4);
        context.Requests.Select(request => request.Authorization).ShouldBe(new[]
        {
            "Bearer token-1", "Bearer token-1", "Bearer token-2", "Bearer token-2"
        });
        context.Requests.Select(request => request.Body).ShouldBe(new[]
        {
            query.Build(0), query.Build(0), query.Build(0), query.Build(2)
        });
        context.Requests.ShouldAllBe(request => request.Address == new Uri("https://example.invalid/v4/games"));
        context.Requests.ShouldAllBe(request => request.ClientId == "test-client");
        context.Clock.GetElapsedTime(context.Requests[0].Timestamp, context.Requests[1].Timestamp)
            .ShouldBeGreaterThanOrEqualTo(TimeSpan.FromSeconds(1));

        for (var index = 1; index < context.Requests.Count; index++)
            context.Clock.GetElapsedTime(context.Requests[index - 1].Timestamp, context.Requests[index].Timestamp)
                .ShouldBeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(300));
    }

    [Fact]
    public async Task QueryAsync_ShouldStopAfterRenewedTokenIsAlsoRejected()
    {
        // Arrange
        using var context = new TestContext();
        context.RespondWith(HttpStatusCode.Unauthorized, "sensitive-marker");
        context.RespondWith(HttpStatusCode.Unauthorized, "sensitive-marker");

        // Act
        var task = context.Client.QueryAsync("games", "fields id;", context.Cancellation.Token);
        await context.Clock.AdvanceNextDelayAsync(TimeSpan.FromMilliseconds(300));

        // Assert
        var exception = await Should.ThrowAsync<HttpRequestException>(() => task.WaitAsync(Watchdog));
        exception.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        exception.ToString().ShouldNotContain("sensitive-marker");
        context.OAuthBodies.Count.ShouldBe(2);
        context.Requests.Count.ShouldBe(2);
    }

    [Fact]
    public async Task QueryAsync_ShouldCancelTransportWhenRealQueryDeadlineExpires()
    {
        // Arrange
        using var context = new TestContext();
        var started = new TaskCompletionSource<CancellationToken>(TaskCreationOptions.RunContinuationsAsynchronously);
        context.IgdbResponse = async (_, token) =>
        {
            started.TrySetResult(token);
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
            return CreateResponse(HttpStatusCode.OK, "[]");
        };

        // Act
        var task = context.Client.QueryAsync("games", "fields id;", context.Cancellation.Token);
        var transportToken = await started.Task.WaitAsync(Watchdog);
        context.Clock.Advance(QueryDeadline);

        // Assert
        var exception = await Should.ThrowAsync<TimeoutException>(() => task.WaitAsync(Watchdog));
        // A watchdog timeout must not masquerade as the production timeout.
        exception.Message.ShouldBe("IGDB query exceeded the configured timeout.");
        transportToken.IsCancellationRequested.ShouldBeTrue();
        context.Cancellation.IsCancellationRequested.ShouldBeFalse();
        context.Requests.Count.ShouldBe(1);
    }

    private static async Task<List<string>> ReadPagesAsync(IgdbPaginator paginator, IgdbPageQuery query, CancellationToken cancellationToken)
    {
        var result = new List<string>();

        await foreach (var page in paginator.ReadPagesAsync("games", query, cancellationToken))
            result.Add(page);

        return result;
    }

    private static HttpResponseMessage CreateResponse(HttpStatusCode status, string body) =>
        new(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

    private sealed record RecordedRequest(
        Uri Address,
        string Authorization,
        string ClientId,
        string Body,
        long Timestamp);

    private sealed class TestContext : IDisposable
    {
        private readonly ServiceProvider _services;
        private readonly Queue<HttpResponseMessage> _responses = new();
        private readonly StubHandler _oauthHandler;
        private readonly StubHandler _igdbHandler;

        public ObservedTimeProvider Clock { get; } = new();
        public CancellationTokenSource Cancellation { get; } = new();
        public List<string> OAuthBodies { get; } = new();
        public List<RecordedRequest> Requests { get; } = new();
        public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>? IgdbResponse { get; set; }
        public IgdbPaginator Paginator => _services.GetRequiredService<IgdbPaginator>();
        public IIgdbClient Client => _services.GetRequiredService<IIgdbClient>();

        public TestContext()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Igdb:ClientId"] = "test-client",
                    ["Igdb:ClientSecret"] = "test-secret",
                    ["Igdb:AuthenticationEndpoint"] = "https://example.invalid/oauth2/token",
                    ["Igdb:ApiBaseAddress"] = "https://example.invalid/v4/",
                    ["Igdb:RequestInterval"] = "00:00:00.300",
                    ["Igdb:RequestTimeout"] = "00:00:30",
                    ["Igdb:MaxRetryAttempts"] = "3"
                })
                .Build();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<TimeProvider>(Clock);
            services.AddIgdbIntegration(configuration);

            _oauthHandler = new StubHandler(SendOAuthAsync);
            _igdbHandler = new StubHandler(SendIgdbAsync);
            // Replace only the transports. Named-client configuration and all services remain real.
            services.AddHttpClient(TwitchOAuthClient.HttpClientName)
                .ConfigurePrimaryHttpMessageHandler(() => _oauthHandler);
            services.AddHttpClient(IgdbClient.HttpClientName)
                .ConfigurePrimaryHttpMessageHandler(() => _igdbHandler);
            _services = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });
        }

        public void RespondWith(HttpStatusCode status, string body, TimeSpan? retryAfter = null)
        {
            var response = CreateResponse(status, body);

            if (retryAfter.HasValue)
                response.Headers.RetryAfter = new RetryConditionHeaderValue(retryAfter.Value);

            _responses.Enqueue(response);
        }

        private async Task<HttpResponseMessage> SendOAuthAsync(HttpRequestMessage request, CancellationToken token)
        {
            request.Method.ShouldBe(HttpMethod.Post);
            request.RequestUri.ShouldBe(new Uri("https://example.invalid/oauth2/token"));
            request.Content!.Headers.ContentType!.MediaType.ShouldBe("application/x-www-form-urlencoded");
            OAuthBodies.Add(await request.Content.ReadAsStringAsync(token));
            var body = "{\"access_token\":\"token-" + OAuthBodies.Count + "\",\"expires_in\":3600,\"token_type\":\"bearer\"}";
            return CreateResponse(HttpStatusCode.OK, body);
        }

        private async Task<HttpResponseMessage> SendIgdbAsync(HttpRequestMessage request, CancellationToken token)
        {
            request.Method.ShouldBe(HttpMethod.Post);
            request.Content!.Headers.ContentType!.MediaType.ShouldBe("text/plain");
            Requests.Add(new RecordedRequest(
                request.RequestUri!,
                request.Headers.Authorization?.ToString() ?? string.Empty,
                request.Headers.GetValues("Client-ID").Single(),
                await request.Content.ReadAsStringAsync(token),
                Clock.GetTimestamp()));

            if (IgdbResponse is not null)
                return await IgdbResponse(request, token);

            if (_responses.Count == 0)
                throw new InvalidOperationException("Unexpected extra IGDB request in test.");

            return _responses.Dequeue();
        }

        public void Dispose()
        {
            Cancellation.Cancel();
            _services.Dispose();
            _oauthHandler.Dispose();
            _igdbHandler.Dispose();

            foreach (var response in _responses)
                response.Dispose();

            Cancellation.Dispose();
        }
    }

    private sealed class StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            send(request, cancellationToken);
    }

    private sealed class ObservedTimeProvider : TimeProvider
    {
        private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 8, 28, 12, 0, 0, TimeSpan.Zero));
        private readonly Channel<TimeSpan> _delays = Channel.CreateUnbounded<TimeSpan>();

        public override long TimestampFrequency => _clock.TimestampFrequency;
        public override DateTimeOffset GetUtcNow() => _clock.GetUtcNow();
        public override long GetTimestamp() => _clock.GetTimestamp();

        public override ITimer CreateTimer(
            TimerCallback callback,
            object? state,
            TimeSpan dueTime,
            TimeSpan period)
        {
            var timer = _clock.CreateTimer(callback, state, dueTime, period);

            // Observe retry/pacing timers, not the fixed 30-second query deadlines.
            if (dueTime > TimeSpan.Zero && dueTime < QueryDeadline)
                _delays.Writer.TryWrite(dueTime);

            return timer;
        }

        public async Task AdvanceNextDelayAsync(TimeSpan expectedDelay)
        {
            // Wait for actual timer registration, not for a guess about async scheduling.
            var delay = await _delays.Reader.ReadAsync().AsTask().WaitAsync(Watchdog);
            delay.ShouldBe(expectedDelay);
            _clock.Advance(delay);
        }

        public void Advance(TimeSpan elapsed) => _clock.Advance(elapsed);
    }
}
