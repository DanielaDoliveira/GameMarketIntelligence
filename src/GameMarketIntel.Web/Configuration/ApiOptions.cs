namespace GameMarketIntel.Web.Configuration;

public sealed class ApiOptions
{
    public const string SectionName = "api";

    public string BaseUrl { get; init; } = string.Empty;
}