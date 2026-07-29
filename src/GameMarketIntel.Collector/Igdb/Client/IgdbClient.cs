using System.Net.Http.Headers;
using System.Net.Http.Json;
using GameMarketIntel.Collector.Igdb.Contracts;
using GameMarketIntel.Collector.Igdb.Poc;
using Microsoft.Extensions.Options;

namespace GameMarketIntel.Collector.Igdb.Client;

public sealed class IgdbClient(
    HttpClient httpClient,
    IOptions<IgdbPocOptions> options)
    : IIgdbClient
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

        response.EnsureSuccessStatusCode();

        var games = await response.Content.ReadFromJsonAsync<List<IgdbGameSample>>(cancellationToken);

        return games ?? [];
    }
}