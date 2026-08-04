using System.Text.Json.Serialization;

namespace GameMarketIntel.Collector.Igdb.Contracts;

public sealed class IgdbReleaseDateReference
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("date")]
    public long? Date { get; set; }

    [JsonPropertyName("human")]
    public string? Human { get; set; }

    [JsonPropertyName("d")]
    public int? Day { get; set; }

    [JsonPropertyName("m")]
    public int? Month { get; set; }

    [JsonPropertyName("y")]
    public int? Year { get; set; }

    [JsonPropertyName("date_format")]
    public IgdbDateFormatReference? DateFormat { get; set; }

    [JsonPropertyName("platform")]
    public IgdbNamedReference? Platform { get; set; }

    [JsonPropertyName("release_region")]
    public IgdbReleaseDateRegionReference? ReleaseRegion { get; set; }

    [JsonPropertyName("status")]
    public IgdbReleaseDateStatusReference? Status { get; set; }
}