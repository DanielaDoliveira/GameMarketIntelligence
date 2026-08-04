using System.Text.Json.Serialization;

namespace GameMarketIntel.Collector.Igdb.Contracts;

public sealed class IgdbReleaseDateRegionReference
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("region")]
    public string Region { get; set; } = string.Empty;
}