namespace GameMarketIntel.Shared.Contracts.Platforms;

public sealed record PlatformDetails(
    Guid Id,
    string Name,
    string? Family,
    string? Manufacturer,
    string? ImageUrl
    );