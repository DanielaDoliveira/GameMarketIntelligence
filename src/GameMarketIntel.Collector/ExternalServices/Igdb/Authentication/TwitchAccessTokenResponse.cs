using System.Text.Json.Serialization;

namespace GameMarketIntel.Collector.ExternalServices.Igdb.Authentication;

internal sealed class TwitchAccessTokenResponse
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; init; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    [JsonPropertyName("token_type")]
    public required string TokenType { get; init; }
}