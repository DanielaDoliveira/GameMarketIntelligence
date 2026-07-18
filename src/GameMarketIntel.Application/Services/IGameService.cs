using GameMarketIntel.Shared.Contracts.Games;

namespace GameMarketIntel.Application.Abstractions.Services;

public interface IGameService
{
    Task<GameDetails> GetByIdAsync(Guid id,CancellationToken cancellationToken = default);
}