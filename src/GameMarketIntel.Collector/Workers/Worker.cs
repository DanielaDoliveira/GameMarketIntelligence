using GameMarketIntel.Collector.Igdb.Authentication;
using GameMarketIntel.Collector.Igdb.Client;
using GameMarketIntel.Collector.Igdb.Contracts;

namespace GameMarketIntel.Collector.Workers;

public sealed class Worker(
    IIgdbAuthenticationService authenticationService,
    IIgdbClient igdbClient,
    IHostApplicationLifetime applicationLifetime,
    ILogger<Worker> logger)
    : BackgroundService
{
    private const int SearchResultLimit = 10;

    private static readonly IReadOnlyList<ReferenceCase> ReferenceCases =
    [
        new(7335, "Splatoon"),
        new(1077, "Super Mario Galaxy"),
        new(26758, "Super Mario Odyssey"),
        new(732, "Grand Theft Auto: San Andreas"),
        new(1020, "Grand Theft Auto V")
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation(
                """
                Starting IGDB targeted game-mode semantics validation:
                Reference cases: {CaseCount}
                Search-result limit per case: {SearchResultLimit}
                """,
                ReferenceCases.Count,
                SearchResultLimit);

            var tokenResponse =
                await authenticationService.GetAccessTokenAsync(stoppingToken);

            if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                throw new InvalidOperationException(
                    "Twitch returned an empty access token.");
            }

            var referenceGames = await igdbClient.GetGamesByIdsAsync(
                tokenResponse.AccessToken,
                ReferenceCases.Select(item => item.GameId).ToArray(),
                stoppingToken);
            var referenceGamesById = referenceGames.ToDictionary(game => game.Id);

            foreach (var reference in ReferenceCases)
            {
                referenceGamesById.TryGetValue(reference.GameId, out var referenceGame);

                var searchResults = await igdbClient.SearchGamesByNameAsync(
                    tokenResponse.AccessToken,
                    reference.SearchTerm,
                    SearchResultLimit,
                    stoppingToken);

                logger.LogInformation(
                    """
                    IGDB game-mode semantics case:
                    Reference ID: {ReferenceId}
                    Search term: {SearchTerm}
                    Reference record: {ReferenceRecord}
                    Related search results returned: {ResultCount}
                    Related search results:
                    {SearchResults}
                    """,
                    reference.GameId,
                    reference.SearchTerm,
                    FormatGame(referenceGame, reference.GameId),
                    searchResults.Count,
                    FormatSearchResults(searchResults, reference.GameId));
            }

            logger.LogInformation(
                """
                IGDB targeted game-mode semantics validation completed.
                Interpretation reminder: game_modes is attached to each IGDB game record,
                not to an individual platform within that record. Search similarity does
                not prove that two returned records represent the same edition.
                """);
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation(
                "IGDB targeted game-mode semantics validation was cancelled.");
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "IGDB targeted game-mode semantics validation failed.");
        }
        finally
        {
            applicationLifetime.StopApplication();
        }
    }

    private static string FormatSearchResults(
        IReadOnlyList<IgdbGameSample> games,
        long referenceId) =>
        games.Count == 0
            ? "- no search results returned"
            : string.Join(Environment.NewLine, games
                .OrderByDescending(game => game.Id == referenceId)
                .ThenBy(game => game.Id)
                .Select(game => $"- {FormatGame(game, referenceId)}"));

    private static string FormatGame(IgdbGameSample? game, long referenceId)
    {
        if (game is null)
        {
            return "not returned";
        }

        return
            $"ID={game.Id}; reference match={FormatBoolean(game.Id == referenceId)}; " +
            $"name={FormatValue(game.Name)}; " +
            $"type={FormatType(game.GameType)}; " +
            $"version parent={FormatReference(game.VersionParent)}; " +
            $"parent game={FormatReference(game.ParentGame)}; " +
            $"platforms={FormatReferences(game.Platforms)}; " +
            $"game modes={FormatReferences(game.GameModes)}";
    }

    private static string FormatType(IgdbGameTypeReference? type) =>
        type is null
            ? "not reported"
            : $"{type.Id}|{FormatValue(type.Type)}";

    private static string FormatReference(IgdbNamedReference? reference) =>
        reference is null
            ? "not reported"
            : $"{reference.Id}|{FormatValue(reference.Name)}";

    private static string FormatReferences(
        IReadOnlyList<IgdbNamedReference>? references) =>
        references is null || references.Count == 0
            ? "not reported"
            : string.Join(", ", references
                .OrderBy(reference => reference.Id)
                .Select(reference =>
                    $"{reference.Id}|{FormatValue(reference.Name)}"));

    private static string FormatBoolean(bool value) => value ? "YES" : "NO";

    private static string FormatValue(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "not reported" : value;

    private sealed record ReferenceCase(long GameId, string SearchTerm);
}
