namespace GameMarketIntel.Application.Games.Search;

public interface IGameSearchRepository
{
    Task<SearchGamesResult> SearchAsync(  SearchGamesQuery query,CancellationToken cancellationToken = default);
}