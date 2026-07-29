using System.Text.Json.Serialization;

namespace GameMarketIntel.Collector.Igdb.Contracts;

public class IgdbGameSample
{
    public IgdbGameSample(string name)
    {
        this.Name = name;
    }

    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("first_release_date")] public long? FirstReleaseDate { get; set; }
    [JsonPropertyName("updated_at")] public long? UpdatedAt { get; set; }
    
}