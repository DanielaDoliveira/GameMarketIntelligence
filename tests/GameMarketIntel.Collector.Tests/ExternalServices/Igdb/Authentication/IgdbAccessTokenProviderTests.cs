using GameMarketIntel.Collector.ExternalServices.Igdb.Authentication;
using NSubstitute;
using Shouldly;

namespace GameMarketIntel.Collector.Tests.ExternalServices.Igdb.Authentication;

public sealed class IgdbAccessTokenProviderTests
{
    [Fact]
    public async Task GetAccessTokenAsync_ShouldReuseValidAccessToken()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 8, 26, 12, 0, 0, TimeSpan.Zero);
        var oAuthClient = Substitute.For<ITwitchOAuthClient>();
        oAuthClient.RequestAccessTokenAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new TwitchAccessToken("access-token", now.AddHours(1))));
        var provider = new IgdbAccessTokenProvider(oAuthClient, new TestTimeProvider(now));

        // Act
        var firstToken = await provider.GetAccessTokenAsync(CancellationToken.None);
        var secondToken = await provider.GetAccessTokenAsync(CancellationToken.None);

        // Assert
        firstToken.ShouldBe("access-token");
        secondToken.ShouldBe("access-token");
        await oAuthClient.Received(1).RequestAccessTokenAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RenewAccessTokenAsync_ShouldReplaceCachedAccessToken()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 8, 26, 12, 0, 0, TimeSpan.Zero);
        var oAuthClient = Substitute.For<ITwitchOAuthClient>();
        oAuthClient.RequestAccessTokenAsync(Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult(new TwitchAccessToken("first-token", now.AddHours(1))),
                Task.FromResult(new TwitchAccessToken("renewed-token", now.AddHours(2))));
        var provider = new IgdbAccessTokenProvider(oAuthClient, new TestTimeProvider(now));

        // Act
        var firstToken = await provider.GetAccessTokenAsync(CancellationToken.None);
        var renewedToken = await provider.RenewAccessTokenAsync(CancellationToken.None);

        // Assert
        firstToken.ShouldBe("first-token");
        renewedToken.ShouldBe("renewed-token");
        await oAuthClient.Received(2).RequestAccessTokenAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAccessTokenAsync_ShouldReplaceTokenInsideExpirationSafetyWindow()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 8, 26, 12, 0, 0, TimeSpan.Zero);
        var timeProvider = new TestTimeProvider(now);
        var oAuthClient = Substitute.For<ITwitchOAuthClient>();
        oAuthClient.RequestAccessTokenAsync(Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult(new TwitchAccessToken("first-token", now.AddMinutes(2))),
                Task.FromResult(new TwitchAccessToken("replacement-token", now.AddHours(1))));
        var provider = new IgdbAccessTokenProvider(oAuthClient, timeProvider);

        // Act
        var firstToken = await provider.GetAccessTokenAsync(CancellationToken.None);
        timeProvider.Advance(TimeSpan.FromSeconds(61));
        var replacementToken = await provider.GetAccessTokenAsync(CancellationToken.None);

        // Assert
        firstToken.ShouldBe("first-token");
        replacementToken.ShouldBe("replacement-token");
        await oAuthClient.Received(2).RequestAccessTokenAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAccessTokenAsync_ShouldPropagateCancellation()
    {
        // Arrange
        using var cancellationSource = new CancellationTokenSource();
        var oAuthClient = Substitute.For<ITwitchOAuthClient>();
        var provider = new IgdbAccessTokenProvider(oAuthClient, TimeProvider.System);
        cancellationSource.Cancel();

        // Act
        Func<Task> action = async () =>
            await provider.GetAccessTokenAsync(cancellationSource.Token);

        // Assert
        await Should.ThrowAsync<OperationCanceledException>(action);
        await oAuthClient.DidNotReceive().RequestAccessTokenAsync(Arg.Any<CancellationToken>());
    }

    private sealed class TestTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void Advance(TimeSpan interval) => _utcNow = _utcNow.Add(interval);
    }
}
