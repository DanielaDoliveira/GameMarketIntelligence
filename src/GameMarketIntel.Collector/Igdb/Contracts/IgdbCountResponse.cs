using System.Text.Json.Serialization;

namespace GameMarketIntel.Collector.Igdb.Contracts;

public sealed class IgdbCountResponse
{
    [JsonPropertyName("count")]
    public int Count { get; set; }
}