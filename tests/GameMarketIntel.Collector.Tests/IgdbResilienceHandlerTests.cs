using System.Net;
using System.Net.Http.Headers;
using GameMarketIntel.Collector.Igdb.Authentication;
using GameMarketIntel.Collector.Igdb.Client;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace GameMarketIntel.Collector.Tests.Igdb.Client;

public sealed class IgdbResilienceHandlerTests
{
    [Test]
    public async Task SendAsync_ShouldUseRetryAfter_WhenRateLimited()
    {
        // Arrange
        var terminalHandler = new SequenceHandler(
            Response(
                HttpStatusCode.TooManyRequests,
                TimeSpan.FromSeconds(2)),
            Response(HttpStatusCode.OK));
        var delay = new RecordingDelay();
        using var client = CreateClient(terminalHandler, delay);

        // Act
        using var response = await client.GetAsync(
            "https://api.igdb.com/v4/games");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        terminalHandler.CallCount.ShouldBe(2);
        delay.Delays.ShouldBe([TimeSpan.FromSeconds(2)]);
    }

    [Test]
    public async Task SendAsync_ShouldUseExponentialBackoff_WhenFailuresAreTransient()
    {
        // Arrange
        var terminalHandler = new SequenceHandler(
            Response(HttpStatusCode.ServiceUnavailable),
            Response(HttpStatusCode.BadGateway),
            Response(HttpStatusCode.OK));
        var delay = new RecordingDelay();
        using var client = CreateClient(terminalHandler, delay);

        // Act
        using var response = await client.GetAsync(
            "https://api.igdb.com/v4/games");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        terminalHandler.CallCount.ShouldBe(3);
        delay.Delays.ShouldBe(
        [
            TimeSpan.FromMilliseconds(250),
            TimeSpan.FromMilliseconds(500)
        ]);
    }

    [TestCase(HttpStatusCode.InternalServerError)]
    [TestCase(HttpStatusCode.BadGateway)]
    [TestCase(HttpStatusCode.ServiceUnavailable)]
    [TestCase(HttpStatusCode.GatewayTimeout)]
    public async Task SendAsync_ShouldRetry_WhenStatusCodeIsTransient(
        HttpStatusCode statusCode)
    {
        // Arrange
        var terminalHandler = new SequenceHandler(
            Response(statusCode),
            Response(HttpStatusCode.OK));
        var delay = new RecordingDelay();
        using var client = CreateClient(terminalHandler, delay);

        // Act
        using var response = await client.GetAsync(
            "https://api.igdb.com/v4/games");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        terminalHandler.CallCount.ShouldBe(2);
        delay.Delays.Count.ShouldBe(1);
    }

    [Test]
    public async Task SendAsync_ShouldReturnLastFailure_WhenRetryLimitIsReached()
    {
        // Arrange
        var terminalHandler = new SequenceHandler(
            Response(HttpStatusCode.ServiceUnavailable),
            Response(HttpStatusCode.ServiceUnavailable),
            Response(HttpStatusCode.ServiceUnavailable),
            Response(HttpStatusCode.ServiceUnavailable));
        var delay = new RecordingDelay();
        using var client = CreateClient(terminalHandler, delay);

        // Act
        using var response = await client.GetAsync(
            "https://api.igdb.com/v4/games");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        terminalHandler.CallCount.ShouldBe(4);
        delay.Delays.Count.ShouldBe(3);
    }

    [Test]
    public async Task SendAsync_ShouldRetry_WhenRequestTimesOut()
    {
        // Arrange
        var terminalHandler = new SequenceHandler(
            new TaskCanceledException("Simulated timeout."),
            Response(HttpStatusCode.OK));
        var delay = new RecordingDelay();
        using var client = CreateClient(terminalHandler, delay);

        // Act
        using var response = await client.GetAsync(
            "https://api.igdb.com/v4/games");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        terminalHandler.CallCount.ShouldBe(2);
        delay.Delays.ShouldBe([TimeSpan.FromMilliseconds(250)]);
    }

    [Test]
    public async Task SendAsync_ShouldStop_WhenCancelledDuringRetryDelay()
    {
        // Arrange
        using var cancellation = new CancellationTokenSource();
        var terminalHandler = new SequenceHandler(
            Response(HttpStatusCode.TooManyRequests));
        var delay = new CancellingDelay(cancellation);
        using var client = CreateClient(terminalHandler, delay);

        // Act
        var action = () => client.GetAsync(
            "https://api.igdb.com/v4/games",
            cancellation.Token);

        // Assert
        await action.ShouldThrowAsync<OperationCanceledException>();
        terminalHandler.CallCount.ShouldBe(1);
    }

    [Test]
    public async Task SendAsync_ShouldRenewTokenOnce_WhenUnauthorized()
    {
        // Arrange
        var terminalHandler = new SequenceHandler(
            Response(HttpStatusCode.Unauthorized),
            Response(HttpStatusCode.OK));
        var authentication = new StubAuthenticationService("renewed-token");
        using var client = CreateClient(
            terminalHandler,
            new RecordingDelay(),
            authentication);
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "https://api.igdb.com/v4/games");
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", "expired-token");

        // Act
        using var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        terminalHandler.CallCount.ShouldBe(2);
        authentication.CallCount.ShouldBe(1);
        terminalHandler.BearerTokens.ShouldBe(
            ["expired-token", "renewed-token"]);
    }

    [Test]
    public async Task SendAsync_ShouldNotLoop_WhenRenewedTokenIsUnauthorized()
    {
        // Arrange
        var terminalHandler = new SequenceHandler(
            Response(HttpStatusCode.Unauthorized),
            Response(HttpStatusCode.Unauthorized));
        var authentication = new StubAuthenticationService("renewed-token");
        using var client = CreateClient(
            terminalHandler,
            new RecordingDelay(),
            authentication);

        // Act
        using var response = await client.GetAsync(
            "https://api.igdb.com/v4/games");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        terminalHandler.CallCount.ShouldBe(2);
        authentication.CallCount.ShouldBe(1);
    }

    private static HttpClient CreateClient(
        HttpMessageHandler terminalHandler,
        IIgdbRetryDelay delay,
        IIgdbAuthenticationService? authentication = null)
    {
        var resilienceHandler = new IgdbResilienceHandler(
            authentication ??
                new StubAuthenticationService("renewed-token"),
            delay,
            NullLogger<IgdbResilienceHandler>.Instance)
        {
            InnerHandler = terminalHandler
        };

        return new HttpClient(resilienceHandler);
    }

    private static HttpResponseMessage Response(
        HttpStatusCode statusCode,
        TimeSpan? retryAfter = null)
    {
        var response = new HttpResponseMessage(statusCode);

        if (retryAfter.HasValue)
        {
            response.Headers.RetryAfter = new RetryConditionHeaderValue(
                retryAfter.Value);
        }

        return response;
    }

    private sealed class RecordingDelay : IIgdbRetryDelay
    {
        public List<TimeSpan> Delays { get; } = [];

        public Task WaitAsync(
            TimeSpan delay,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Delays.Add(delay);

            return Task.CompletedTask;
        }
    }

    private sealed class CancellingDelay(
        CancellationTokenSource cancellation) : IIgdbRetryDelay
    {
        public Task WaitAsync(
            TimeSpan delay,
            CancellationToken cancellationToken)
        {
            cancellation.Cancel();

            return Task.Delay(
                Timeout.InfiniteTimeSpan,
                cancellationToken);
        }
    }

    private sealed class StubAuthenticationService(
        string accessToken) : IIgdbAuthenticationService
    {
        public int CallCount { get; private set; }

        public Task<IgdbAccessTokenResponse> GetAccessTokenAsync(
            CancellationToken cancellationToken = default)
        {
            CallCount++;

            return Task.FromResult(
                new IgdbAccessTokenResponse
                {
                    AccessToken = accessToken,
                    ExpiresIn = 3600,
                    TokenType = "bearer"
                });
        }
    }

    private sealed class SequenceHandler(
        params object[] outcomes) : HttpMessageHandler
    {
        private readonly Queue<object> _outcomes = new(outcomes);

        public int CallCount { get; private set; }
        public List<string?> BearerTokens { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            BearerTokens.Add(request.Headers.Authorization?.Parameter);

            if (_outcomes.Count == 0)
            {
                throw new InvalidOperationException(
                    "No simulated outcome remains.");
            }

            return _outcomes.Dequeue() switch
            {
                HttpResponseMessage response =>
                    Task.FromResult(response),
                Exception exception =>
                    Task.FromException<HttpResponseMessage>(exception),
                _ => throw new InvalidOperationException(
                    "Unsupported simulated outcome.")
            };
        }
    }
}
