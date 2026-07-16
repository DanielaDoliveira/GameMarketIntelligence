namespace GameMarketIntel.Shared.Requests.Games;

public sealed class SearchGamesRequest
{
    public string? Name { get; init; }

    public Guid[] GenreIds { get; init; } = [];

    public Guid[] PlatformIds { get; init; } = [];
}