using GameMarketIntel.Application.Abstractions.Persistence;
using GameMarketIntel.Application.Abstractions.Services;
using GameMarketIntel.Shared.Contracts.Platforms;

namespace GameMarketIntel.Application.Services;

public sealed class PlatformService(
    IPlatformRepository platformRepository) : IPlatformService
{
    public async Task<IReadOnlyList<PlatformDetails>> GetAllAsync( CancellationToken cancellationToken = default)
    {
        var platforms =  await platformRepository.GetAllAsync(cancellationToken);

        return platforms
            .Select(platform => new PlatformDetails(
                platform.Id,
                platform.Name,
                platform.Family,
                platform.Manufacturer,
                platform.ImageUrl))
            .ToList();
    }
}