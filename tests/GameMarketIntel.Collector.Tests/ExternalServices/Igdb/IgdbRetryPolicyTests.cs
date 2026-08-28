using System.Net;
using System.Net.Http.Headers;
using GameMarketIntel.Collector.ExternalServices.Igdb;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Shouldly;

namespace GameMarketIntel.Collector.Tests.ExternalServices.Igdb;

public sealed class IgdbRetryPolicyTests
{
    private static readonly DateTimeOffset StartTime =
        new(2026, 8, 27, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(HttpStatusCode.RequestTimeout)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    public void GetRetryDelay_ShouldUseExponentialBackoff_ForTransientStatus(HttpStatusCode statusCode)
    {
        // Arrange
        using var response = new HttpResponseMessage(statusCode);
        var policy = CreatePolicy();

        // Act
        var retryDelay = policy.GetRetryDelay(response, retryAttempt: 2);

        // Assert
        retryDelay.ShouldBe(TimeSpan.FromSeconds(4));
    }

    [Fact]
    public void GetRetryDelay_ShouldNotRetryNonTransientStatus()
    {
        // Arrange
        using var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        var policy = CreatePolicy();

        // Act
        var retryDelay = policy.GetRetryDelay(response, retryAttempt: 0);

        // Assert
        retryDelay.ShouldBeNull();
    }

    [Fact]
    public void GetRetryDelay_ShouldNotRetryAfterMaximumAttempts()
    {
        // Arrange
        using var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        var policy = CreatePolicy(maxRetryAttempts: 3);

        // Act
        var retryDelay = policy.GetRetryDelay(response, retryAttempt: 3);

        // Assert
        retryDelay.ShouldBeNull();
    }

    [Fact]
    public void GetRetryDelay_ShouldPreferRetryAfterDelta()
    {
        // Arrange
        var expectedDelay = TimeSpan.FromSeconds(10);
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        response.Headers.RetryAfter = new RetryConditionHeaderValue(expectedDelay);
        var policy = CreatePolicy();

        // Act
        var retryDelay = policy.GetRetryDelay(response, retryAttempt: 0);

        // Assert
        retryDelay.ShouldBe(expectedDelay);
    }

    [Fact]
    public void GetRetryDelay_ShouldCalculateDelayFromRetryAfterDate()
    {
        // Arrange
        var expectedDelay = TimeSpan.FromSeconds(10);
        using var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        response.Headers.RetryAfter = new RetryConditionHeaderValue(StartTime.Add(expectedDelay));
        var policy = CreatePolicy();

        // Act
        var retryDelay = policy.GetRetryDelay(response, retryAttempt: 0);

        // Assert
        retryDelay.ShouldBe(expectedDelay);
    }

    [Fact]
    public void GetRetryDelay_ShouldNotRetryWhenDelayExceedsConfiguredMaximum()
    {
        // Arrange
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(31));
        var policy = CreatePolicy(maxRetryDelay: TimeSpan.FromSeconds(30));

        // Act
        var retryDelay = policy.GetRetryDelay(response, retryAttempt: 0);

        // Assert
        retryDelay.ShouldBeNull();
    }

    [Fact]
    public void GetRetryDelay_ShouldRejectNegativeRetryAttempt()
    {
        // Arrange
        using var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        var policy = CreatePolicy();

        // Act
        Action action = () => policy.GetRetryDelay(response, retryAttempt: -1);

        // Assert
        Should.Throw<ArgumentOutOfRangeException>(action);
    }

    private static IgdbRetryPolicy CreatePolicy(int maxRetryAttempts = 3, TimeSpan? maxRetryDelay = null)
    {
        var options = Options.Create(new IgdbOptions
        {
            ClientId = "client-id",
            ClientSecret = "client-secret",
            MaxRetryAttempts = maxRetryAttempts,
            MaxRetryDelay = maxRetryDelay ?? TimeSpan.FromSeconds(30)
        });

        return new IgdbRetryPolicy(options, new FakeTimeProvider(StartTime));
    }
}
