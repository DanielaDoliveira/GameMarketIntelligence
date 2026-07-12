namespace GameMarketIntel.Shared.Contracts.Sources;

public sealed record DataSourceDetails(
    Guid Id,
    string Name,
    string Url,
    string? LicenseNotes,
    bool AttributionRequired,
    SourceReliabilityDetails Reliability);