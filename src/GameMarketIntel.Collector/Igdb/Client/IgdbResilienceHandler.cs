using System.Net;
using System.Net.Http.Headers;
using GameMarketIntel.Collector.Igdb.Authentication;

namespace GameMarketIntel.Collector.Igdb.Client;

public sealed class IgdbResilienceHandler(
    IIgdbAuthenticationService authenticationService,
    IIgdbRetryDelay retryDelay,
    ILogger<IgdbResilienceHandler> logger) : DelegatingHandler
{
    private const int MaximumRetries = 3;
    private static readonly TimeSpan InitialBackoff =
        TimeSpan.FromMilliseconds(250);
    private static readonly TimeSpan MaximumRetryDelay =
        TimeSpan.FromSeconds(30);

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var snapshot = await RequestSnapshot.CreateAsync(
            request,
            cancellationToken);
        AuthenticationHeaderValue? refreshedAuthorization = null;
        var tokenWasRefreshed = false;
        var retryNumber = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var attemptRequest = snapshot.CreateRequest(
                refreshedAuthorization);

            try
            {
                var response = await base.SendAsync(
                    attemptRequest,
                    cancellationToken);

                if (response.StatusCode == HttpStatusCode.Unauthorized &&
                    !tokenWasRefreshed)
                {
                    response.Dispose();

                    var token = await authenticationService.GetAccessTokenAsync(
                        cancellationToken);

                    if (string.IsNullOrWhiteSpace(token.AccessToken))
                    {
                        throw new InvalidOperationException(
                            "Twitch returned an empty access token during IGDB reauthentication.");
                    }

                    refreshedAuthorization =
                        new AuthenticationHeaderValue(
                            "Bearer",
                            token.AccessToken);
                    tokenWasRefreshed = true;

                    logger.LogWarning(
                        "IGDB returned HTTP 401. The access token was renewed once.");

                    continue;
                }

                if (!IsTransient(response.StatusCode) ||
                    retryNumber >= MaximumRetries)
                {
                    return response;
                }

                var delay = GetDelay(response, retryNumber);
                var statusCode = response.StatusCode;
                response.Dispose();
                retryNumber++;

                logger.LogWarning(
                    "IGDB returned HTTP {StatusCode}. " +
                    "Retry {RetryNumber}/{MaximumRetries} will start " +
                    "after {DelayMs} ms.",
                    (int)statusCode,
                    retryNumber,
                    MaximumRetries,
                    delay.TotalMilliseconds);

                await retryDelay.WaitAsync(delay, cancellationToken);
            }
            catch (TaskCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                if (retryNumber >= MaximumRetries)
                {
                    throw;
                }

                var delay = GetExponentialBackoff(retryNumber);
                retryNumber++;

                logger.LogWarning(
                    "IGDB request timed out. " +
                    "Retry {RetryNumber}/{MaximumRetries} will start " +
                    "after {DelayMs} ms.",
                    retryNumber,
                    MaximumRetries,
                    delay.TotalMilliseconds);

                await retryDelay.WaitAsync(delay, cancellationToken);
            }
        }
    }

    private static bool IsTransient(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.TooManyRequests
            or HttpStatusCode.InternalServerError
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;

    private static TimeSpan GetDelay(
        HttpResponseMessage response,
        int retryNumber)
    {
        if (response.Headers.RetryAfter?.Delta is { } delta &&
            delta > TimeSpan.Zero)
        {
            return Min(delta, MaximumRetryDelay);
        }

        if (response.Headers.RetryAfter?.Date is { } date)
        {
            var remaining = date - DateTimeOffset.UtcNow;

            if (remaining > TimeSpan.Zero)
            {
                return Min(remaining, MaximumRetryDelay);
            }
        }

        return GetExponentialBackoff(retryNumber);
    }

    private static TimeSpan GetExponentialBackoff(int retryNumber) =>
        Min(
            TimeSpan.FromMilliseconds(
                InitialBackoff.TotalMilliseconds *
                Math.Pow(2, retryNumber)),
            MaximumRetryDelay);

    private static TimeSpan Min(TimeSpan first, TimeSpan second) =>
        first <= second ? first : second;

    private sealed record RequestSnapshot(
        HttpMethod Method,
        Uri? RequestUri,
        Version Version,
        HttpVersionPolicy VersionPolicy,
        IReadOnlyList<KeyValuePair<string, IEnumerable<string>>> Headers,
        byte[]? Content,
        IReadOnlyList<KeyValuePair<string, IEnumerable<string>>> ContentHeaders)
    {
        public static async Task<RequestSnapshot> CreateAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var content = request.Content is null
                ? null
                : await request.Content.ReadAsByteArrayAsync(
                    cancellationToken);

            return new RequestSnapshot(
                request.Method,
                request.RequestUri,
                request.Version,
                request.VersionPolicy,
                request.Headers.ToArray(),
                content,
                request.Content?.Headers.ToArray() ?? []);
        }

        public HttpRequestMessage CreateRequest(
            AuthenticationHeaderValue? authorizationOverride)
        {
            var request = new HttpRequestMessage(Method, RequestUri)
            {
                Version = Version,
                VersionPolicy = VersionPolicy
            };

            foreach (var header in Headers)
            {
                request.Headers.TryAddWithoutValidation(
                    header.Key,
                    header.Value);
            }

            if (authorizationOverride is not null)
            {
                request.Headers.Authorization = authorizationOverride;
            }

            if (Content is not null)
            {
                request.Content = new ByteArrayContent(Content);

                foreach (var header in ContentHeaders)
                {
                    request.Content.Headers.TryAddWithoutValidation(
                        header.Key,
                        header.Value);
                }
            }

            return request;
        }
    }
}
