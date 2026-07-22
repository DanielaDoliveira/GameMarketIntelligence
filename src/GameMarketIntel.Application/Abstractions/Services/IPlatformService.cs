using GameMarketIntel.Shared.Contracts.Platforms;

namespace GameMarketIntel.Application.Abstractions.Services;

public interface IPlatformService
{
    Task<IReadOnlyList<PlatformDetails>> GetAllAsync(CancellationToken cancellationToken = default);
}