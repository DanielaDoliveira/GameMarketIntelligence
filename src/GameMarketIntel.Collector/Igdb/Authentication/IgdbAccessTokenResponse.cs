using System.Text.Json.Serialization;

namespace GameMarketIntel.Collector.Igdb.Authentication;

public sealed class IgdbAccessTokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = string.Empty;
    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
    [JsonPropertyName("token_type")] public string TokenType { get; set; } = string.Empty;
    
}