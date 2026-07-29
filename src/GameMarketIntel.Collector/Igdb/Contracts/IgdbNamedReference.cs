using System.Text.Json.Serialization;

namespace GameMarketIntel.Collector.Igdb.Contracts;

public sealed class IgdbNamedReference
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}