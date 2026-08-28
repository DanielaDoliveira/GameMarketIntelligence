using System.Net;
using System.Text;
using GameMarketIntel.Collector.ExternalServices.Igdb;
using GameMarketIntel.Collector.ExternalServices.Igdb.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;

namespace GameMarketIntel.Collector.Tests.ExternalServices.Igdb.Authentication;

public sealed class TwitchOAuthClientTests
{
    [Fact]
    public async Task RequestAccessTokenAsync_ShouldReturnValidatedAccessToken()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 8, 26, 12, 0, 0, TimeSpan.Zero);
        var timeProvider = new TestTimeProvider(now);
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            request.Method.ShouldBe(HttpMethod.Post);
            request.RequestUri.ShouldBe(new Uri("https://id.twitch.tv/oauth2/token"));

            return Task.FromResult(CreateResponse(
                HttpStatusCode.OK,
                """{"access_token":"access-token","expires_in":3600,"token_type":"bearer"}"""));
        });
        var client = CreateClient(handler, timeProvider);

        // Act
        var token = await client.RequestAccessTokenAsync(CancellationToken.None);

        // Assert
        token.Value.ShouldBe("access-token");
        token.ExpiresAt.ShouldBe(now.AddHours(1));
        handler.CallCount.ShouldBe(1);
    }

    [Fact]
    public async Task RequestAccessTokenAsync_ShouldNotExposeResponseBody_WhenRequestFails()
    {
        // Arrange
        const string sensitiveResponse = "client-secret-must-not-appear";
        var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(CreateResponse(HttpStatusCode.Unauthorized, sensitiveResponse)));
        var client = CreateClient(handler);

        // Act
        var exception = await Should.ThrowAsync<HttpRequestException>(() =>
            client.RequestAccessTokenAsync(CancellationToken.None));

        // Assert
        exception.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        exception.Message.ShouldNotContain(sensitiveResponse);
    }

    [Theory]
    [InlineData("{\"access_token\":\"\",\"expires_in\":3600,\"token_type\":\"bearer\"}")]
    [InlineData("{\"access_token\":\"access-token\",\"expires_in\":0,\"token_type\":\"bearer\"}")]
    [InlineData("{\"access_token\":\"access-token\",\"expires_in\":3600,\"token_type\":\"other\"}")]
    public async Task RequestAccessTokenAsync_ShouldRejectInvalidResponse(string responseBody)
    {
        // Arrange
        var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(CreateResponse(HttpStatusCode.OK, responseBody)));
        var client = CreateClient(handler);

        // Act
        var action = () => client.RequestAccessTokenAsync(CancellationToken.None);

        // Assert
        await Should.ThrowAsync<InvalidDataException>(action);
    }

    [Fact]
    public async Task RequestAccessTokenAsync_ShouldPropagateCancellation()
    {
        // Arrange
        using var cancellationSource = new CancellationTokenSource();
        var handler = new StubHttpMessageHandler(async (_, cancellationToken) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return CreateResponse(HttpStatusCode.OK, "{}");
        });
        var client = CreateClient(handler);
        cancellationSource.Cancel();

        // Act
        Func<Task> action = async () =>
            await client.RequestAccessTokenAsync(cancellationSource.Token);

        // Assert
        await Should.ThrowAsync<OperationCanceledException>(action);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("<html>sensitive-marker</html>")]
    [InlineData("{")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("""{"access_token":"sensitive-marker","expires_in":"not-a-number","token_type":"bearer"}""")]
    [InlineData("""{"access_token":{"sensitive-marker":true},"expires_in":3600,"token_type":"bearer"}""")]
    [InlineData("""{"access_token":"sensitive-marker","expires_in":3600}""")]
    [InlineData("""{"access_token":"token","expires_in":3600,"token_type":"bearer","sensitive-marker":[}""")]
    public async Task RequestAccessTokenAsync_ShouldSanitizeMalformedOrIncompatibleJson(string responseBody)
    {
        // Arrange
        var logger = Substitute.For<ILogger<TwitchOAuthClient>>();
        using var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(CreateResponse(HttpStatusCode.OK, responseBody)));
        var client = CreateClient(handler, logger: logger);

        // Act
        var action = () => client.RequestAccessTokenAsync(CancellationToken.None);

        // Assert
        var exception = await Should.ThrowAsync<InvalidDataException>(action);
        exception.Message.ShouldBe("Twitch OAuth returned malformed or incompatible token JSON.");
        exception.InnerException.ShouldBeNull();
        exception.ToString().ShouldNotContain("sensitive-marker");
        handler.CallCount.ShouldBe(1);

        foreach (var call in logger.ReceivedCalls())
        {
            foreach (var argument in call.GetArguments())
            {
                (argument?.ToString() ?? string.Empty).ShouldNotContain("sensitive-marker");
                (argument is Exception).ShouldBeFalse();
            }
        }
    }

    [Fact]
    public async Task RequestAccessTokenAsync_ShouldRejectJsonNullWithoutParserDetails()
    {
        // Arrange
        using var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(CreateResponse(HttpStatusCode.OK, "null")));
        var client = CreateClient(handler);

        // Act
        var action = () => client.RequestAccessTokenAsync(CancellationToken.None);

        // Assert
        var exception = await Should.ThrowAsync<InvalidDataException>(action);
        exception.Message.ShouldBe("Twitch OAuth returned an empty access token response.");
        exception.InnerException.ShouldBeNull();
    }

    private static TwitchOAuthClient CreateClient(HttpMessageHandler handler, TimeProvider? timeProvider = null, ILogger<TwitchOAuthClient>? logger = null)
    {
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient(TwitchOAuthClient.HttpClientName)
            .Returns(new HttpClient(handler));
        var options = Options.Create(new IgdbOptions
        {
            ClientId = "client-id",
            ClientSecret = "client-secret"
        });

        return new TwitchOAuthClient(
            httpClientFactory,
            options,
            timeProvider ?? TimeProvider.System,
            logger ?? Substitute.For<ILogger<TwitchOAuthClient>>());
    }

    private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, string body) =>
        new(statusCode)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFactory) : HttpMessageHandler
    {
        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            return responseFactory(request, cancellationToken);
        }
    }

    private sealed class TestTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
