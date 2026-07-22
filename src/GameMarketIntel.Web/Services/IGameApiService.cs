using GameMarketIntel.Shared.Contracts.Games;
using GameMarketIntel.Shared.Contracts.Games.Search;
using GameMarketIntel.Shared.Requests.Games;

namespace GameMarketIntel.Web.Services;

public interface IGameApiService
{
    Task<SearchGamesResult> SearchAsync( SearchGamesRequest request, CancellationToken cancellationToken = default);

    Task<GameDetails?> GetByIdAsync( Guid id, CancellationToken cancellationToken = default);
}