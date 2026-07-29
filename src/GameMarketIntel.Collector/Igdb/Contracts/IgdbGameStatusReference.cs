using System.Text.Json.Serialization;

namespace GameMarketIntel.Collector.Igdb.Contracts;

public sealed class IgdbGameStatusReference
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}