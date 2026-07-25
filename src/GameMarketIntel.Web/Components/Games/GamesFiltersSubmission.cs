namespace GameMarketIntel.Web.Components.Games;

public sealed record GamesFiltersSubmission(
    string? SearchTerm,
    Guid? GenreId,
    Guid? PlatformId,
    int? ReleaseYear);