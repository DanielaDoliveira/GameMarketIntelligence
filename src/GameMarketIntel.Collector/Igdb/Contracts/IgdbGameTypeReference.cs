using System.Text.Json.Serialization;

namespace GameMarketIntel.Collector.Igdb.Contracts;

public sealed class IgdbGameTypeReference
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}