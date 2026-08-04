using System.Text.Json.Serialization;

namespace GameMarketIntel.Collector.Igdb.Contracts;

public sealed class IgdbDateFormatReference
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("format")]
    public string Format { get; set; } = string.Empty;
}