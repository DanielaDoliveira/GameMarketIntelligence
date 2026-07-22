using GameMarketIntel.Application.Games.Search;
using GameMarketIntel.Shared.Contracts.Games.Search;

namespace GameMarketIntel.Application.Abstractions.Services;

public interface IGameSearchService
{
    Task<SearchGamesResult> SearchAsync( SearchGamesQuery query,CancellationToken cancellationToken = default);
}