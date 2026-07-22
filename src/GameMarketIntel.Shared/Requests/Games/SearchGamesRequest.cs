namespace GameMarketIntel.Shared.Requests.Games;

public sealed class SearchGamesRequest
{
    public string? Search { get; init; }

    public Guid? GenreId { get; init; }

    public Guid? PlatformId { get; init; }

    public int? ReleaseYear { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}