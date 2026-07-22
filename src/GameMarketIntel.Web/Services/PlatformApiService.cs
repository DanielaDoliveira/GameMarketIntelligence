using System.Net.Http.Json;
using GameMarketIntel.Shared.Contracts.Platforms;

namespace GameMarketIntel.Web.Services;

public sealed class PlatformApiService(
    HttpClient httpClient) : IPlatformApiService
{
    public async Task<IReadOnlyList<PlatformDetails>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var platforms = await httpClient.GetFromJsonAsync<List<PlatformDetails>>("api/platforms", cancellationToken);

        return platforms ?? [];
    }
}