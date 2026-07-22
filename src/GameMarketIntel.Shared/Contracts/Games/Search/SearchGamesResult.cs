namespace GameMarketIntel.Shared.Contracts.Games.Search;

public sealed record SearchGamesResult(
    IReadOnlyCollection<GameSearchItem> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);