using System.Net;
using System.Net.Http;
using System.Text;
using GameMarketIntel.Collector.ExternalServices;
using GameMarketIntel.Collector.ExternalServices.Igdb;
using GameMarketIntel.Collector.ExternalServices.Igdb.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Shouldly;

namespace GameMarketIntel.Collector.Tests.ExternalServices.Igdb;

public sealed class IgdbClientTests
{
    private const string Query = "fields id,name; limit 1;";

    [Fact]
    public async Task QueryAsync_ShouldSendAuthenticatedPostAndReturnJson()
    {
        // Arrange
        using var context = new TestContext(async (request, cancellationToken) =>
        {
            request.Method.ShouldBe(HttpMethod.Post);
            request.RequestUri.ShouldBe(new Uri("https://example.invalid/v4/games"));
            request.Headers.GetValues("Client-ID").Single().ShouldBe("test-client-id");
            request.Headers.Authorization!.Scheme.ShouldBe("Bearer");
            request.Headers.Authorization!.Parameter.ShouldBe("initial-token");
            request.Content!.Headers.ContentType!.MediaType.ShouldBe("text/plain");
            (await request.Content!.ReadAsStringAsync(cancellationToken)).ShouldBe(Query);
            return CreateResponse(HttpStatusCode.OK, "[{\"id\":1}]");
        });

        // Act
        var result = await context.Client.QueryAsync("games", Query, CancellationToken.None);

        // Assert
        result.ShouldBe("[{\"id\":1}]");
        await context.Pacer.Received(1).WaitAsync(Arg.Any<CancellationToken>());
        await context.Tokens.DidNotReceive().RenewAccessTokenAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueryAsync_ShouldRenewTokenAndReplaySameQueryAfterUnauthorized()
    {
        // Arrange
        var tokens = new List<string?>();
        var bodies = new List<string>();
        using var context = new TestContext(async (request, cancellationToken) =>
        {
            tokens.Add(request.Headers.Authorization!.Parameter);
            bodies.Add(await request.Content!.ReadAsStringAsync(cancellationToken));
            return CreateResponse(tokens.Count == 1 ? HttpStatusCode.Unauthorized : HttpStatusCode.OK);
        });

        // Act
        await context.Client.QueryAsync("games", Query, CancellationToken.None);

        // Assert
        tokens.Count.ShouldBe(2);
        tokens[0].ShouldBe("initial-token");
        tokens[1].ShouldBe("renewed-token");
        bodies.ShouldAllBe(body => body == Query);
        await context.Tokens.Received(1).RenewAccessTokenAsync(Arg.Any<CancellationToken>());
        await context.Pacer.Received(2).WaitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueryAsync_ShouldStopAfterSecondUnauthorizedResponse()
    {
        // Arrange
        using var context = new TestContext((_, _) => Task.FromResult(CreateResponse(HttpStatusCode.Unauthorized)));

        // Act
        var exception = await Should.ThrowAsync<HttpRequestException>(() =>
            context.Client.QueryAsync("games", Query, CancellationToken.None));

        // Assert
        exception.StatusCode.ShouldBe((HttpStatusCode?)HttpStatusCode.Unauthorized);
        context.Handler.CallCount.ShouldBe(2);
        await context.Tokens.Received(1).RenewAccessTokenAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueryAsync_ShouldWaitForRetryPolicyDelayAndPaceEveryAttempt()
    {
        // Arrange
        var calls = 0;
        using var context = new TestContext((_, _) => Task.FromResult(CreateResponse(
            ++calls == 1 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK)));
        context.Retry.GetRetryDelay(Arg.Any<HttpResponseMessage>(), 0)
            .Returns((TimeSpan?)TimeSpan.FromSeconds(2));

        // Act
        var task = context.Client.QueryAsync("games", Query, CancellationToken.None);
        context.Clock.Advance(TimeSpan.FromSeconds(1));
        var callsBeforeDelayElapsed = calls;
        context.Clock.Advance(TimeSpan.FromSeconds(1));
        var result = await task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        callsBeforeDelayElapsed.ShouldBe(1);
        calls.ShouldBe(2);
        result.ShouldBe("[]");
        await context.Pacer.Received(2).WaitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueryAsync_ShouldStopWhenPolicyRejectsAnotherRetry()
    {
        // Arrange
        using var context = new TestContext((_, _) => Task.FromResult(CreateResponse(HttpStatusCode.ServiceUnavailable)));
        context.Retry.GetRetryDelay(Arg.Any<HttpResponseMessage>(), 0).Returns((TimeSpan?)TimeSpan.Zero);
        context.Retry.GetRetryDelay(Arg.Any<HttpResponseMessage>(), 1).Returns((TimeSpan?)null);

        // Act
        var exception = await Should.ThrowAsync<HttpRequestException>(() =>
            context.Client.QueryAsync("games", Query, CancellationToken.None));

        // Assert
        exception.StatusCode.ShouldBe((HttpStatusCode?)HttpStatusCode.ServiceUnavailable);
        context.Handler.CallCount.ShouldBe(2);
        context.Retry.Received(1).GetRetryDelay(Arg.Any<HttpResponseMessage>(), 1);
    }

    [Fact]
    public async Task QueryAsync_ShouldNotExposeFailedResponseBody()
    {
        // Arrange
        const string sensitiveText = "synthetic-secret";
        using var context = new TestContext((_, _) => Task.FromResult(CreateResponse(HttpStatusCode.BadRequest, sensitiveText)));

        // Act
        var exception = await Should.ThrowAsync<HttpRequestException>(() =>
            context.Client.QueryAsync("games", Query, CancellationToken.None));

        // Assert
        exception.Message.ShouldBe("IGDB request failed with status code 400.");
        exception.ToString().ShouldNotContain(sensitiveText);
        context.Handler.CallCount.ShouldBe(1);
    }

    [Fact]
    public async Task QueryAsync_ShouldIncludeTokenAcquisitionInTimeout()
    {
        // Arrange
        using var context = new TestContext((_, _) => Task.FromResult(CreateResponse(HttpStatusCode.OK)));
        context.Tokens.GetAccessTokenAsync(Arg.Any<CancellationToken>())
            .Returns(call => WaitForCancellationAsync(call.Arg<CancellationToken>()));

        // Act
        var task = context.Client.QueryAsync("games", Query, CancellationToken.None);
        context.Clock.Advance(TimeSpan.FromSeconds(30));

        // Assert
        var exception = await Should.ThrowAsync<TimeoutException>(() => task.WaitAsync(TimeSpan.FromSeconds(5)));
        exception.Message.ShouldBe("IGDB query exceeded the configured timeout.");
        context.Handler.CallCount.ShouldBe(0);
    }

    [Fact]
    public async Task QueryAsync_ShouldCancelHttpRequestAtDeadline()
    {
        // Arrange
        var requestCancelled = false;
        using var context = new TestContext(async (_, cancellationToken) =>
        {
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                return CreateResponse(HttpStatusCode.OK);
            }
            finally
            {
                requestCancelled = cancellationToken.IsCancellationRequested;
            }
        });

        // Act
        var task = context.Client.QueryAsync("games", Query, CancellationToken.None);
        context.Clock.Advance(TimeSpan.FromSeconds(30));

        // Assert
        var exception = await Should.ThrowAsync<TimeoutException>(() => task.WaitAsync(TimeSpan.FromSeconds(5)));
        exception.Message.ShouldBe("IGDB query exceeded the configured timeout.");
        requestCancelled.ShouldBeTrue();
        context.Handler.CallCount.ShouldBe(1);
    }

    [Fact]
    public async Task QueryAsync_ShouldPreserveCallerCancellation()
    {
        // Arrange
        using var context = new TestContext(async (_, cancellationToken) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return CreateResponse(HttpStatusCode.OK);
        });
        using var cancellationSource = new CancellationTokenSource();

        // Act
        var task = context.Client.QueryAsync("games", Query, cancellationSource.Token);
        await cancellationSource.CancelAsync();

        // Assert
        await Should.ThrowAsync<OperationCanceledException>(() => task.WaitAsync(TimeSpan.FromSeconds(5)));
        context.Handler.CallCount.ShouldBe(1);
        await context.Tokens.DidNotReceive().RenewAccessTokenAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("https://other.invalid/games")]
    [InlineData("//other.invalid/games")]
    [InlineData("../games")]
    [InlineData("games?secret=value")]
    [InlineData("games#fragment")]
    [InlineData("games%2fcount")]
    public void QueryAsync_ShouldRejectInvalidResourceNameBeforeHttp(string endpoint)
    {
        // Arrange
        using var context = new TestContext((_, _) => Task.FromResult(CreateResponse(HttpStatusCode.OK)));

        // Act
        Action action = () => context.Client.QueryAsync(endpoint, Query, CancellationToken.None);

        // Assert
        Should.Throw<ArgumentException>(action);
        context.Handler.CallCount.ShouldBe(0);
        context.Factory.DidNotReceive().CreateClient(Arg.Any<string>());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void QueryAsync_ShouldRejectEmptyQuery(string query)
    {
        // Arrange
        using var context = new TestContext((_, _) => Task.FromResult(CreateResponse(HttpStatusCode.OK)));

        // Act
        Action action = () => context.Client.QueryAsync("games", query, CancellationToken.None);

        // Assert
        Should.Throw<ArgumentException>(action);
        context.Handler.CallCount.ShouldBe(0);
    }

    private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, string body = "[]") =>
        new(statusCode) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

    private static async Task<string> WaitForCancellationAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        return string.Empty;
    }

    private sealed class TestContext : IDisposable
    {
        public FakeTimeProvider Clock { get; } = new();
        public IHttpClientFactory Factory { get; } = Substitute.For<IHttpClientFactory>();
        public IIgdbAccessTokenProvider Tokens { get; } = Substitute.For<IIgdbAccessTokenProvider>();
        public IIgdbRequestPacer Pacer { get; } = Substitute.For<IIgdbRequestPacer>();
        public IIgdbRetryPolicy Retry { get; } = Substitute.For<IIgdbRetryPolicy>();
        public StubHttpMessageHandler Handler { get; }
        public IgdbClient Client { get; }

        public TestContext(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFactory)
        {
            Handler = new StubHttpMessageHandler(responseFactory);
            Factory.CreateClient(IgdbClient.HttpClientName).Returns(_ => CreateHttpClient());
            Tokens.GetAccessTokenAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult("initial-token"));
            Tokens.RenewAccessTokenAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult("renewed-token"));
            Pacer.WaitAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            Retry.GetRetryDelay(Arg.Any<HttpResponseMessage>(), Arg.Any<int>()).Returns((TimeSpan?)null);
            var options = Options.Create(new IgdbOptions { RequestTimeout = TimeSpan.FromSeconds(30) });
            Client = new IgdbClient(
                Factory,
                Tokens,
                Pacer,
                Retry,
                new IgdbQueryTimeout(options, Clock),
                Clock);
        }

        public void Dispose() => Handler.Dispose();

        private HttpClient CreateHttpClient()
        {
            // Simulates the named-client configuration that will be added to DI.
            var client = new HttpClient(Handler, disposeHandler: false)
            {
                BaseAddress = new Uri("https://example.invalid/v4/"),
                Timeout = Timeout.InfiniteTimeSpan
            };
            client.DefaultRequestHeaders.Add("Client-ID", "test-client-id");
            return client;
        }
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFactory) : HttpMessageHandler
    {
        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            return responseFactory(request, cancellationToken);
        }
    }
}
