namespace GameMarketIntel.Collector.Igdb.Poc;

public class IgdbPocOptions
{
    public const string SectionName = "Igbd";
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public int SampleSize { get; set; } = 10;
    
    
}