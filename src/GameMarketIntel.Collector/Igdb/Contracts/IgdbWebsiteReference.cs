using System.Text.Json.Serialization;

namespace GameMarketIntel.Collector.Igdb.Contracts;

public sealed class IgdbWebsiteReference
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("trusted")]
    public bool Trusted { get; set; }

    [JsonPropertyName("type")]
    public IgdbWebsiteTypeReference? Type { get; set; }
}