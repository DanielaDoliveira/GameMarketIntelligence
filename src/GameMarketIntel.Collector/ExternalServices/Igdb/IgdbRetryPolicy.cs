using System.Net;
using Microsoft.Extensions.Options;

namespace GameMarketIntel.Collector.ExternalServices.Igdb;

public sealed class IgdbRetryPolicy(
    IOptions<IgdbOptions> options,
    TimeProvider timeProvider) : IIgdbRetryPolicy
{
    private readonly IgdbOptions _options = options.Value;

    public TimeSpan? GetRetryDelay(HttpResponseMessage response, int retryAttempt)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentOutOfRangeException.ThrowIfNegative(retryAttempt);

        if (!IsTransient(response.StatusCode) || retryAttempt >= _options.MaxRetryAttempts)
            return null;

        var retryDelay = ReadRetryAfter(response) ?? CalculateBackoff(retryAttempt);

        return retryDelay <= _options.MaxRetryDelay
            ? retryDelay
            : null;
    }

    private TimeSpan? ReadRetryAfter(HttpResponseMessage response)
    {
        var retryAfter = response.Headers.RetryAfter;
        var delta = retryAfter?.Delta;

        if (delta.HasValue && delta.Value >= TimeSpan.Zero)
            return delta.Value;

        var retryDate = retryAfter?.Date;

        if (!retryDate.HasValue)
            return null;

        var delay = retryDate.Value - timeProvider.GetUtcNow();
        return delay > TimeSpan.Zero ? delay : TimeSpan.Zero;
    }
    private static TimeSpan CalculateBackoff(int retryAttempt) =>
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
    private static bool IsTransient(HttpStatusCode statusCode) =>
        statusCode
            is HttpStatusCode.RequestTimeout or
            HttpStatusCode.TooManyRequests or
            HttpStatusCode.InternalServerError or
            HttpStatusCode.BadGateway or
            HttpStatusCode.ServiceUnavailable or
            HttpStatusCode.GatewayTimeout;
}