using GameMarketIntel.Shared.Contracts.Platforms;

namespace GameMarketIntel.Web.Services;

public interface IPlatformApiService
{
    Task<IReadOnlyList<PlatformDetails>> GetAllAsync( CancellationToken cancellationToken = default);
}