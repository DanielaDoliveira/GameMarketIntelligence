using GameMarketIntel.Application.Games.Search;

namespace GameMarketIntel.Application.Abstractions.Services;

public interface IGameSearchService
{
    Task<SearchGamesResult> SearchAsync( SearchGamesQuery query,CancellationToken cancellationToken = default);
}