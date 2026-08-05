using System.Net.Http.Headers;
using System.Net.Http.Json;
using GameMarketIntel.Collector.Igdb.Contracts;
using GameMarketIntel.Collector.Igdb.Poc;
using Microsoft.Extensions.Options;

namespace GameMarketIntel.Collector.Igdb.Client;


public sealed class IgdbClient(HttpClient httpClient, IOptions<IgdbPocOptions> options) : IIgdbClient
{
    private readonly IgdbPocOptions _options = options.Value;

    public async Task<int> CountReleasedGamesAsync(string accessToken, long releaseDateCutoff, CancellationToken cancellationToken = default)
    {
        if (releaseDateCutoff <= 0)
        {
            throw new ArgumentOutOfRangeException
            (
                nameof(releaseDateCutoff), 
                "The release-date cutoff must be a positive Unix timestamp."
            );
        }

        using var request = new HttpRequestMessage
        (
            HttpMethod.Post,
            "https://api.igdb.com/v4/games/count"
         );

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("Client-ID", _options.ClientId);
        request.Content = new StringContent(
            $"""
             where first_release_date != null
                 & first_release_date < {releaseDateCutoff};
             """);

        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new HttpRequestException(
                $"""
                 IGDB games-count request failed.
                 Status code: {(int)response.StatusCode} ({response.StatusCode})
                 Response: {errorContent}
                 """,
                inner: null,
                response.StatusCode);
        }

        var countResponse =
            await response.Content.ReadFromJsonAsync<IgdbCountResponse>(cancellationToken);

        return countResponse?.Count ?? throw new InvalidOperationException("IGDB returned an empty games-count response.");
    }

   public async Task<IReadOnlyList<IgdbGameSample>>
    GetReleasedGameAtOffsetAsync(
        string accessToken,
        long releaseDateCutoff,
        int offset,
        CancellationToken cancellationToken = default)
{
    if (releaseDateCutoff <= 0)
    {
        throw new ArgumentOutOfRangeException(
            nameof(releaseDateCutoff),
            "The release-date cutoff must be a positive Unix timestamp.");
    }

    if (offset < 0)
    {
        throw new ArgumentOutOfRangeException(
            nameof(offset),
            "The offset cannot be negative.");
    }

    using var request = CreateGamesRequest(
        accessToken,
        $"""
         fields
             id,
             name,
             first_release_date;

         where first_release_date != null
             & first_release_date < {releaseDateCutoff};

         sort id asc;
         offset {offset};
         limit 1;
         """);

    return await SendGamesRequestAsync(
        request,
        cancellationToken);
}

public async Task<IReadOnlyList<IgdbGameSample>>
    GetReleasedGamesAtOffsetsAsync(
        string accessToken,
        long releaseDateCutoff,
        IReadOnlyCollection<int> offsets,
        CancellationToken cancellationToken = default)
{
    ArgumentNullException.ThrowIfNull(offsets);

    if (releaseDateCutoff <= 0)
    {
        throw new ArgumentOutOfRangeException(
            nameof(releaseDateCutoff),
            "The release-date cutoff must be a positive Unix timestamp.");
    }

    if (offsets.Count == 0)
    {
        return [];
    }

    if (offsets.Any(offset => offset < 0))
    {
        throw new ArgumentOutOfRangeException(
            nameof(offsets),
            "Offsets cannot be negative.");
    }

    if (offsets.Distinct().Count() != offsets.Count)
    {
        throw new ArgumentException(
            "Offsets must be distinct.",
            nameof(offsets));
    }

    var games = new List<IgdbGameSample>(offsets.Count);

    foreach (var offset in offsets)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var gamesAtOffset =
            await GetReleasedGameAtOffsetAsync(
                accessToken,
                releaseDateCutoff,
                offset,
                cancellationToken);

        if (gamesAtOffset.Count != 1)
        {
            throw new InvalidOperationException(
                $"""
                 Expected exactly one IGDB game at offset {offset},
                 but received {gamesAtOffset.Count}.
                 """);
        }

        games.Add(gamesAtOffset[0]);
    }

    return games;
}

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
        if (gameIds.Count == 0)
        {
            return [];
        }

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
                 keywords.name,

                 involved_companies.id,
                 involved_companies.company.id,
                 involved_companies.company.name,
                 involved_companies.developer,
                 involved_companies.publisher,
                 involved_companies.porting,
                 involved_companies.supporting,

                 external_games.id,
                 external_games.name,
                 external_games.uid,
                 external_games.url,
                 external_games.external_game_source.id,
                 external_games.external_game_source.name,
                 external_games.platform.id,
                 external_games.platform.name,

                 websites.id,
                 websites.url,
                 websites.trusted,
                 websites.type.id,
                 websites.type.type,
                 
                 release_dates.id,
                 release_dates.date,
                 release_dates.human,
                 release_dates.d,
                 release_dates.m,
                 release_dates.y,
                 release_dates.date_format.id,
                 release_dates.date_format.format,
                 release_dates.platform.id,
                 release_dates.platform.name,
                 release_dates.release_region.id,
                 release_dates.release_region.region,
                 release_dates.status.id,
                 release_dates.status.name,
                 release_dates.status.description;

             where id = ({formattedIds});
             sort id asc;
             limit {gameIds.Count};
             """);

        return await SendGamesRequestAsync(request, cancellationToken);
    }

    public async Task<IReadOnlyList<IgdbGameSample>> GetGamesWithParentAsync(string accessToken, int sampleSize, CancellationToken cancellationToken = default)
    {
        using var request = CreateGamesRequest(accessToken,
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
             where parent_game != null;
             sort updated_at desc;
             limit {sampleSize};
             """);
        return await SendGamesRequestAsync(request, cancellationToken);
    }

    public async Task<IReadOnlyList<IgdbGameSample>> GetGamesByTypeAsync(string accessToken, long gameTypeId, int sampleSize,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateGamesRequest(accessToken,
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
                 keywords.name,
                 bundles.id,
                 bundles.name;
             where game_type = {gameTypeId};
             sort updated_at desc;
             limit {sampleSize};
             """);

        return await SendGamesRequestAsync(request, cancellationToken);
    }

    public async Task<IReadOnlyList<IgdbGameTypeReference>> GetGameTypesAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.igdb.com/v4/game_types");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("Client-ID", _options.ClientId);
        request.Content = new StringContent(
            """
            fields
                id,
                type;
            sort id asc;
            limit 500;
            """);
        
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new HttpRequestException(
                $"""
                 IGDB game-types request failed.
                 Status code: {(int)response.StatusCode} ({response.StatusCode})
                 Response: {errorContent}
                 """,
                inner: null,
                response.StatusCode
                );
        }
        var gameTypes = await response.Content.ReadFromJsonAsync<List<IgdbGameTypeReference>>(cancellationToken);

        return gameTypes ?? [];
    }

    public async Task<IReadOnlyList<IgdbGameSample>>
        GetGamesIncludedInBundleAsync(string accessToken, long bundleId, CancellationToken cancellationToken = default)
    {
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

                 version_parent.id,
                 version_parent.name,

                 parent_game.id,
                 parent_game.name,

                 bundles.id,
                 bundles.name,

                 platforms.id,
                 platforms.name,

                 release_dates.id,
                 release_dates.date,
                 release_dates.human,
                 release_dates.d,
                 release_dates.m,
                 release_dates.y,
                 release_dates.date_format.id,
                 release_dates.date_format.format,
                 release_dates.platform.id,
                 release_dates.platform.name,
                 release_dates.release_region.id,
                 release_dates.release_region.region,
                 release_dates.status.id,
                 release_dates.status.name,
                 release_dates.status.description;

             where bundles = {bundleId};
             sort id asc;
             limit 50;
             """);

        return await SendGamesRequestAsync(request, cancellationToken);
    }

    public async Task<IReadOnlyList<IgdbGameSample>> SearchGamesByNameAsync(string accessToken, string gameName, int resultLimit, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(gameName))
        {
            return [];
        }

        if (resultLimit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(resultLimit), "The result limit must be greater than zero.");
        }

        var escapedGameName = gameName
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");

        using var request = CreateGamesRequest(
            accessToken,
            $"""
             search "{escapedGameName}";
             fields
                 id,
                 name,
                 first_release_date,
                 game_type.id,
                 game_type.type,
                 version_parent.id,
                 version_parent.name,
                 parent_game.id,
                 parent_game.name,
                 platforms.id,
                 platforms.name;
             limit {resultLimit};
             """);

        return await SendGamesRequestAsync(request, cancellationToken);
    }

    public async Task<IReadOnlyList<IgdbGameSample>> GetGamesAlternativeNamesSampleAsync(string accessToken, IReadOnlyCollection<long> gameIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(gameIds);

        if (gameIds.Count == 0)
        {
            return [];
        }

        if (gameIds.Count > 500)
        {
            throw new ArgumentOutOfRangeException(nameof(gameIds), "An IGDB games query cannot request more than 500 IDs.");
        }

        if (gameIds.Any(gameId => gameId <= 0))
        {
            throw new ArgumentOutOfRangeException(nameof(gameIds), "Game IDs must be positive.");
        }

        if (gameIds.Distinct().Count() != gameIds.Count)
        {
            throw new ArgumentException("Game IDs must be distinct.", nameof(gameIds));
        }

        var formattedIds = string.Join(",", gameIds);

        using var request = CreateGamesRequest(
            accessToken,
            $"""
             fields
                 id,
                 name,
                 version_title,

                 alternative_names.id,
                 alternative_names.name,
                 alternative_names.comment,

                 game_localizations.id,
                 game_localizations.name,
                 game_localizations.region.id,
                 game_localizations.region.name,
                 game_localizations.region.identifier,
                 game_localizations.region.category;

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