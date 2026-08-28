using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using GameMarketIntel.Collector.ExternalServices.Igdb.Authentication;

namespace GameMarketIntel.Collector.ExternalServices.Igdb;

public sealed class IgdbClient(
    IHttpClientFactory httpClientFactory,
    IIgdbAccessTokenProvider accessTokenProvider,
    IIgdbRequestPacer requestPacer,
    IIgdbRetryPolicy retryPolicy,
    IgdbQueryTimeout queryTimeout,
    TimeProvider timeProvider) : IIgdbClient
{
    public const string HttpClientName = "Igdb";

    public Task<string> QueryAsync(string endpoint, string query, CancellationToken cancellationToken)
    {
        ValidateRequest(endpoint, query);
        return queryTimeout.ExecuteAsync(token => ExecuteQueryAsync(endpoint, query, token), cancellationToken);
    }

    private async Task<string> ExecuteQueryAsync(string endpoint, string query, CancellationToken cancellationToken)
    {
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        var accessToken = await accessTokenProvider.GetAccessTokenAsync(cancellationToken);
        var authenticationRenewed = false;
        var retryAttempt = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await requestPacer.WaitAsync(cancellationToken);

            bool shouldRenewToken;
            var retryDelay = TimeSpan.Zero;

            using (var request = CreateRequest(endpoint, query, accessToken))
            using (var response =
                   await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync(cancellationToken);

                shouldRenewToken = response.StatusCode == HttpStatusCode.Unauthorized && !authenticationRenewed;

                if (!shouldRenewToken)
                {
                    var proposedDelay = retryPolicy.GetRetryDelay(response, retryAttempt);

                    if (!proposedDelay.HasValue)
                        throw IgdbRequestFailureFactory.CreateException(response);

                    retryDelay = proposedDelay.Value;
                }
            }

            // Release the response before renewing credentials or waiting for a retry.
            if (shouldRenewToken)
            {
                authenticationRenewed = true;
                accessToken = await accessTokenProvider.RenewAccessTokenAsync(cancellationToken);
            }
            else
            {
                retryAttempt++;
                await Task.Delay(retryDelay, timeProvider, cancellationToken);
            }
        }
    }

    private static HttpRequestMessage CreateRequest(string endpoint, string query, string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(query, Encoding.UTF8, "text/plain")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private static void ValidateRequest(string endpoint, string query)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        // Accept a resource name, not a URL, path, query string, or fragment.
        if (endpoint.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '_'))
            throw new ArgumentException("IGDB endpoint must contain only ASCII letters, digits, or underscores.",
                nameof(endpoint));
    }
}