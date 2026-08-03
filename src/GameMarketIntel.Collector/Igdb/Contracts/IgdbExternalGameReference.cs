using System.Text.Json.Serialization;

namespace GameMarketIntel.Collector.Igdb.Contracts;

public sealed class IgdbExternalGameReference
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("uid")]
    public string? ExternalId { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("external_game_source")]
    public IgdbNamedReference? Source { get; set; }

    [JsonPropertyName("platform")]
    public IgdbNamedReference? Platform { get; set; }
}