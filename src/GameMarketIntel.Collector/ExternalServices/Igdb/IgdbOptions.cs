namespace GameMarketIntel.Collector.ExternalServices.Igdb;

public sealed class IgdbOptions
{
    public const string SectionName = "Igdb";
    public Uri AuthenticationEndpoint { get; init; } = new("https://id.twitch.tv/oauth2/token");
    public Uri ApiBaseAddress { get; init; } = new("https://api.igdb.com/v4/");
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public int PageSize { get; init; } = 500;
    public TimeSpan RequestInterval { get; init; } = TimeSpan.FromMilliseconds(300);
    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(30);
    public int MaxRetryAttempts { get; init; } = 3;
    public TimeSpan MaxRetryDelay { get; init; } = TimeSpan.FromSeconds(30);
}