namespace GameMarketIntel.Shared.Contracts.Games.Search;

public sealed record GameSearchItem(
    Guid Id,
    string Name,
    string? Description,
    DateOnly? ReleaseDate,
    string? ImageUrl,
    IReadOnlyCollection<GameSearchCategory> Genres,
    IReadOnlyCollection<GameSearchCategory> Platforms);