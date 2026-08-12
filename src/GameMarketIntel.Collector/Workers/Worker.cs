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
    private const int SampleSeed = 20260804;
    private const string SampleCutoff = "2026-08-04T00:00:00Z";

    private static readonly long[] SampleGameIds =
    [
        5722, 8916, 10646, 11845, 15671, 26254, 34755, 35086, 36409,
        37586, 43038, 48588, 50982, 53080, 75951, 85339, 87336, 88673,
        94203, 102352, 104269, 110942, 112378, 112780, 115420, 121801,
        127166, 132372, 136185, 136414, 138954, 147667, 148596, 161412,
        167238, 168966, 171880, 175803, 178134, 183126, 183605, 198307,
        200355, 204839, 211968, 212672, 215569, 235524, 235982, 236449,
        248613, 250264, 257727, 263132, 268601, 269217, 275556, 282139,
        284455, 284464, 285194, 288072, 293820, 299137, 303375, 306988,
        313806, 317392, 317584, 318108, 326470, 327816, 328786, 329403,
        334302, 336294, 337379, 339924, 341133, 347230, 349475, 350477,
        360889, 361018, 367296, 369479, 370585, 376941, 377320, 378090,
        379032, 380648, 382347, 385917, 391762, 396823, 397730, 397831,
        402731, 403794
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation(
                """
                Starting IGDB random-sample game-modes analysis:
                Frozen sample size: {SampleCount}
                Sample seed: {SampleSeed}
                Sample cutoff: {SampleCutoff}
                """,
                SampleGameIds.Length,
                SampleSeed,
                SampleCutoff);

            var tokenResponse =
                await authenticationService.GetAccessTokenAsync(stoppingToken);

            if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                throw new InvalidOperationException(
                    "Twitch returned an empty access token.");
            }

            var games = await igdbClient.GetGamesByIdsAsync(
                tokenResponse.AccessToken,
                SampleGameIds,
                stoppingToken);

            LogResults(games);
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation(
                "IGDB random-sample game-modes analysis was cancelled.");
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "IGDB random-sample game-modes analysis failed.");
        }
        finally
        {
            applicationLifetime.StopApplication();
        }
    }

    private void LogResults(IReadOnlyList<IgdbGameSample> games)
    {
        var gamesById = games.ToDictionary(game => game.Id);
        var returnedGames = SampleGameIds
            .Where(gamesById.ContainsKey)
            .Select(gameId => gamesById[gameId])
            .ToArray();

        var resultLines = SampleGameIds.Select(gameId =>
            gamesById.TryGetValue(gameId, out var game)
                ? FormatResult(game)
                : $"- ID={gameId}; record=NOT RETURNED");

        logger.LogInformation(
            "IGDB random-sample game-modes results:{NewLine}{Results}",
            Environment.NewLine,
            string.Join(Environment.NewLine, resultLines));

        var gamesWithModes = returnedGames
            .Where(game => game.GameModes.Count > 0)
            .ToArray();
        var recordsWithDuplicateIds = returnedGames.Count(game =>
            game.GameModes.Select(item => item.Id).Distinct().Count()
            != game.GameModes.Count);
        var recordsWithInvalidValues = returnedGames.Count(game =>
            game.GameModes.Any(item =>
                item.Id <= 0 || string.IsNullOrWhiteSpace(item.Name)));
        var conflictingNamesById = returnedGames
            .SelectMany(game => game.GameModes)
            .GroupBy(item => item.Id)
            .Count(group => group
                .Select(item => item.Name?.Trim())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count() > 1);
        var distinctModes = returnedGames
            .SelectMany(game => game.GameModes)
            .GroupBy(item => item.Id)
            .Select(group => new
            {
                Id = group.Key,
                Name = group.Select(item => item.Name).FirstOrDefault(name =>
                    !string.IsNullOrWhiteSpace(name)) ?? "not reported",
                RecordCount = returnedGames.Count(game =>
                    game.GameModes.Any(item => item.Id == group.Key))
            })
            .OrderByDescending(item => item.RecordCount)
            .ThenBy(item => item.Id)
            .ToArray();

        var frequencyLines = distinctModes.Length == 0
            ? "- no game modes reported"
            : string.Join(Environment.NewLine, distinctModes.Select(item =>
                $"- {item.Id}|{item.Name}: {item.RecordCount}/{returnedGames.Length} " +
                $"records ({FormatPercentage(item.RecordCount, returnedGames.Length)})"));

        logger.LogInformation(
            """
            IGDB random-sample game-modes summary:
            Frozen sample size: {SampleCount}
            Records returned: {ReturnedCount}
            Missing records: {MissingCount}
            Records with one or more game modes: {WithModeCount}
            Records without game modes: {WithoutModeCount}
            Game-mode presence coverage among returned records: {Coverage}
            Records with exactly one game mode: {ExactlyOneCount}
            Records with multiple game modes: {MultipleCount}
            Records with duplicate game-mode IDs: {DuplicateCount}
            Records with invalid game-mode IDs or blank names: {InvalidCount}
            Game-mode IDs with conflicting reported names: {ConflictingNameCount}
            Distinct game-mode IDs reported: {DistinctCount}

            Game-mode frequency among returned records:
            {FrequencyLines}
            """,
            SampleGameIds.Length,
            returnedGames.Length,
            SampleGameIds.Length - returnedGames.Length,
            gamesWithModes.Length,
            returnedGames.Length - gamesWithModes.Length,
            FormatPercentage(gamesWithModes.Length, returnedGames.Length),
            returnedGames.Count(game => game.GameModes.Count == 1),
            returnedGames.Count(game => game.GameModes.Count > 1),
            recordsWithDuplicateIds,
            recordsWithInvalidValues,
            conflictingNamesById,
            distinctModes.Length,
            frequencyLines);

        logger.LogInformation(
            "IGDB random-sample game-modes analysis completed.");
    }

    private static string FormatResult(IgdbGameSample game)
    {
        var duplicateIds = game.GameModes
            .Select(item => item.Id)
            .Distinct()
            .Count() != game.GameModes.Count;
        var invalidValues = game.GameModes.Any(item =>
            item.Id <= 0 || string.IsNullOrWhiteSpace(item.Name));

        return
            $"- ID={game.Id}; title={FormatValue(game.Name)}; " +
            $"game modes={FormatReferences(game.GameModes)}; " +
            $"mode count={game.GameModes.Count}; " +
            $"duplicate IDs={FormatBoolean(duplicateIds)}; " +
            $"invalid values={FormatBoolean(invalidValues)}";
    }

    private static string FormatReferences(
        IReadOnlyList<IgdbNamedReference> references) =>
        references.Count == 0
            ? "not reported"
            : string.Join(", ", references
                .OrderBy(reference => reference.Id)
                .Select(reference =>
                    $"{reference.Id}|{FormatValue(reference.Name)}"));

    private static string FormatPercentage(int numerator, int denominator) =>
        denominator == 0
            ? "0.00%"
            : $"{(double)numerator / denominator:P2}";

    private static string FormatBoolean(bool value) => value ? "YES" : "NO";

    private static string FormatValue(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "not reported" : value;
}
