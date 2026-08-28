using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace GameMarketIntel.Collector.ExternalServices.Igdb.Authentication;

public sealed class TwitchOAuthClient(
    IHttpClientFactory httpClientFactory,
    IOptions<IgdbOptions> options,
    TimeProvider timeProvider,
    ILogger<TwitchOAuthClient> logger):ITwitchOAuthClient
{
    public const string HttpClientName = "TwitchOAuth";
    private readonly IgdbOptions _options = options.Value;

    public async Task<TwitchAccessToken> RequestAccessTokenAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Requesting an IGDB access token from Twitch OAuth.");

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["grant_type"] = "client_credentials"
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.AuthenticationEndpoint)
        {
            Content = content
        };

        var httpClient = httpClientFactory.CreateClient(HttpClientName);
        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "Twitch OAuth rejected the IGDB authentication request with status code {StatusCode}.",
                (int)response.StatusCode);

            throw new HttpRequestException(
                $"Twitch OAuth request failed with status code {(int)response.StatusCode}.",
                null,
                response.StatusCode);
        }

        var tokenResponse = await ReadResponseAsync(response.Content, cancellationToken);

        ValidateResponse(tokenResponse);

        var expiresAt = timeProvider.GetUtcNow().AddSeconds(tokenResponse!.ExpiresIn);

        logger.LogInformation("IGDB access token acquired successfully.");

        return new TwitchAccessToken(tokenResponse.AccessToken, expiresAt);
    }

    private static async Task<TwitchAccessTokenResponse?> ReadResponseAsync(HttpContent content, CancellationToken cancellationToken)
    {
        try
        {
            return await content.ReadFromJsonAsync<TwitchAccessTokenResponse>(cancellationToken);
        }
        catch (JsonException)
        {
            // Parser messages and paths can contain remote response data.
            // Do not log or attach the original exception.
            throw new InvalidDataException("Twitch OAuth returned malformed or incompatible token JSON.");
        }
    }

    private static void ValidateResponse(TwitchAccessTokenResponse? response)
    {
        if (response is null || string.IsNullOrWhiteSpace(response.AccessToken))
            throw new InvalidDataException("Twitch OAuth returned an empty access token response.");

        if (response.ExpiresIn <= 0)
            throw new InvalidDataException("Twitch OAuth returned an invalid access token lifetime.");

        if (!string.Equals(response.TokenType, "bearer", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Twitch OAuth returned an unsupported access token type.");
    }
}