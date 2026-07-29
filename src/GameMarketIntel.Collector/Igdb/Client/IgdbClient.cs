using System.Net.Http.Headers;
using System.Net.Http.Json;
using GameMarketIntel.Collector.Igdb.Contracts;
using GameMarketIntel.Collector.Igdb.Poc;
using Microsoft.Extensions.Options;

namespace GameMarketIntel.Collector.Igdb.Client;

public sealed class IgdbClient(HttpClient httpClient, IOptions<IgdbPocOptions> options) : IIgdbClient
{
    private readonly IgdbPocOptions _options = options.Value;

    public async Task<IReadOnlyList<IgdbGameSample>> GetGamesSampleAsync(string accessToken, int sampleSize, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.igdb.com/v4/games");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        request.Headers.Add("Client-ID", _options.ClientId);
        request.Content = new StringContent(
            $"""
             fields
                 id,
                 name,
                 first_release_date,
                 updated_at,
                 game_type.id,
                 game_type.type,
                 game_status.id,
                 game_status.status,
                 version_parent.id,
                 version_parent.name,
                 parent_game.id,
                 parent_game.name,
                 platforms.id,
                 platforms.name,
                 genres.id,
                 genres.name,
                 themes.id,
                 themes.name,
                 keywords.id,
                 keywords.name;
             sort updated_at desc;
             limit {sampleSize};
             """);

        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new HttpRequestException(
                $"""
                 IGDB request failed.
                 Status code: {(int)response.StatusCode} ({response.StatusCode})
                 Response: {errorContent}
                 """,
                inner: null,
                response.StatusCode);
        }

        var games = await response.Content.ReadFromJsonAsync<List<IgdbGameSample>>(cancellationToken);

        return games ?? [];
    }

    public async Task<IReadOnlyList<IgdbGameSample>> GetGamesByIdsAsync(string accessToken, IReadOnlyCollection<long> gameIds, CancellationToken cancellationToken = default)
    {
        if (gameIds.Count == 0) return [];
        var formattedIds = string.Join(",", gameIds);
        using var request = CreateGamesRequest(
            accessToken,
            $"""
             fields
                 id,
                 name,
                 first_release_date,
                 updated_at,
                 game_type.id,
                 game_type.type,
                 game_status.id,
                 game_status.status,
                 version_parent.id,
                 version_parent.name,
                 parent_game.id,
                 parent_game.name,
                 platforms.id,
                 platforms.name,
                 genres.id,
                 genres.name,
                 themes.id,
                 themes.name,
                 keywords.id,
                 keywords.name;
             where id = ({formattedIds});
             sort id asc;
             limit {gameIds.Count};
             """);
        return await SendGamesRequestAsync(request, cancellationToken);

    }

    private HttpRequestMessage CreateGamesRequest(string accessToken, string query)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.igdb.com/v4/games");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("Client-ID",_options.ClientId);
        request.Content = new StringContent(query);
        return request;
    }

    private async Task<IReadOnlyList<IgdbGameSample>> SendGamesRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            throw new HttpRequestException(
                $"""
                 IGDB request failed.
                 Status code: {(int)response.StatusCode} ({response.StatusCode})
                 Response: {errorContent}
                 """,
                inner: null,
                response.StatusCode
                );
        }

        var games = await response.Content.ReadFromJsonAsync<List<IgdbGameSample>>(cancellationToken);
        return games ?? [];
    }
}