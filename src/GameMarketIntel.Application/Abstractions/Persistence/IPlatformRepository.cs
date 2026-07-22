using GameMarketIntel.Domain.Entities;

namespace GameMarketIntel.Application.Abstractions.Persistence;

public interface IPlatformRepository
{
    Task<IReadOnlyList<Platform>> GetAllAsync( CancellationToken cancellationToken = default);
}