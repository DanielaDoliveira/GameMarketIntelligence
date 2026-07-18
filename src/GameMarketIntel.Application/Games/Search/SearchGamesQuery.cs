namespace GameMarketIntel.Application.Games.Search;

public sealed record SearchGamesQuery(
    string? Search,
    Guid? GenreId,
    Guid? PlatformId,
    int? ReleaseYear,
    int Page = 1,
    int PageSize = 20);