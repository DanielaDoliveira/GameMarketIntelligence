using GameMarketIntel.Shared.Contracts.Games;
using GameMarketIntel.Shared.Requests.Games;

namespace GameMarketIntel.Application.Abstractions.Services;

public interface IGameService
{
    Task<SearchGamesResponse> SearchAsync(
        SearchGamesRequest request,
        CancellationToken cancellationToken = default);

    Task<GameDetails?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}