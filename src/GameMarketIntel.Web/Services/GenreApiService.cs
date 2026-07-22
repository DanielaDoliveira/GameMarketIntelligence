using System.Net.Http.Json;
using GameMarketIntel.Shared.Contracts.Genres;

namespace GameMarketIntel.Web.Services;

public sealed class GenreApiService( HttpClient httpClient) : IGenreApiService
{
    public async Task<IReadOnlyList<GenreDetails>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var genres = await httpClient.GetFromJsonAsync<List<GenreDetails>>("api/genres", cancellationToken);

        return genres ?? [];
    }
}