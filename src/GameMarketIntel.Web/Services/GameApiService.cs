using System.Net;
using System.Net.Http.Json;
using GameMarketIntel.Shared.Contracts.Games;
using GameMarketIntel.Shared.Contracts.Games.Search;
using GameMarketIntel.Shared.Requests.Games;

namespace GameMarketIntel.Web.Services;

public sealed class GameApiService(HttpClient httpClient) : IGameApiService
{
    public async Task<SearchGamesResult> SearchAsync( SearchGamesRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var queryParameters = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            queryParameters.Add($"search={Uri.EscapeDataString(request.Search.Trim())}");
        }

        if (request.GenreId.HasValue)
        {
            queryParameters.Add($"genreId={request.GenreId.Value}");
        }

        if (request.PlatformId.HasValue)
        {
            queryParameters.Add($"platformId={request.PlatformId.Value}");
        }

        if (request.ReleaseYear.HasValue)
        {
            queryParameters.Add($"releaseYear={request.ReleaseYear.Value}");
        }

        queryParameters.Add($"page={request.Page}");
        queryParameters.Add($"pageSize={request.PageSize}");

        var requestUri = $"api/games?{string.Join("&", queryParameters)}";

        var result =
            await httpClient.GetFromJsonAsync<SearchGamesResult>(
                requestUri,
                cancellationToken);

        return result
            ?? new SearchGamesResult(
                [],
                request.Page,
                request.PageSize,
                0,
                0);
    }

    public async Task<GameDetails?> GetByIdAsync( Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"api/games/{id}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content .ReadFromJsonAsync<GameDetails>( cancellationToken);
    }
}