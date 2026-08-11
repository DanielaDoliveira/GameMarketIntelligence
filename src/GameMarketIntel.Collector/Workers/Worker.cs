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
    private static readonly IReadOnlyList<long> SelectedGameIds =
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
            ValidateSelectedGameIds();

            logger.LogInformation(
                """
                Starting IGDB franchises-and-collections analysis:
                Fixed sample size: {SampleSize}
                Original release-date cutoff: {ReleaseDateCutoff}
                Original sample seed: {SampleSeed}
                Original eligible population: {EligiblePopulation}
                """,
                SelectedGameIds.Count,
                "2026-08-04T00:00:00Z",
                20260804,
                278772);

            var tokenResponse =
                await authenticationService.GetAccessTokenAsync(stoppingToken);

            if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                throw new InvalidOperationException(
                    "Twitch returned an empty access token.");
            }

            var games = await igdbClient.GetGamesByIdsAsync(
                tokenResponse.AccessToken,
                SelectedGameIds,
                stoppingToken);

            ValidateReturnedGames(games);
            LogGameEvidence(games);
            LogSummary(games);
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation(
                "IGDB franchises-and-collections analysis was cancelled.");
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "IGDB franchises-and-collections analysis failed.");
        }
        finally
        {
            applicationLifetime.StopApplication();
        }
    }

    private static void ValidateSelectedGameIds()
    {
        if (SelectedGameIds.Count != 100)
        {
            throw new InvalidOperationException(
                $"Expected 100 fixed sample IDs, but found {SelectedGameIds.Count}.");
        }

        if (SelectedGameIds.Any(gameId => gameId <= 0))
        {
            throw new InvalidOperationException(
                "The fixed sample contains a non-positive game ID.");
        }

        if (SelectedGameIds.Distinct().Count() != SelectedGameIds.Count)
        {
            throw new InvalidOperationException(
                "The fixed sample contains duplicate game IDs.");
        }
    }

    private static void ValidateReturnedGames(IReadOnlyList<IgdbGameSample> games)
    {
        var duplicateIds = games
            .GroupBy(game => game.Id)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .Order()
            .ToArray();

        var returnedIds = games.Select(game => game.Id).ToHashSet();
        var selectedIds = SelectedGameIds.ToHashSet();
        var missingIds = SelectedGameIds
            .Where(gameId => !returnedIds.Contains(gameId))
            .Order()
            .ToArray();
        var unexpectedIds = returnedIds
            .Where(gameId => !selectedIds.Contains(gameId))
            .Order()
            .ToArray();

        if (duplicateIds.Length == 0 &&
            missingIds.Length == 0 &&
            unexpectedIds.Length == 0 &&
            games.Count == SelectedGameIds.Count)
        {
            return;
        }

        throw new InvalidOperationException(
            $"""
             IGDB did not return the fixed sample exactly as requested.
             Requested games: {SelectedGameIds.Count}
             Returned games: {games.Count}
             Missing IDs: {FormatIds(missingIds)}
             Unexpected IDs: {FormatIds(unexpectedIds)}
             Duplicate returned IDs: {FormatIds(duplicateIds)}
             """);
    }

    private void LogGameEvidence(IReadOnlyList<IgdbGameSample> games)
    {
        foreach (var game in games.OrderBy(game => game.Id))
        {
            logger.LogInformation(
                """
                IGDB franchises-and-collections evidence:
                Game ID: {GameId}
                Primary name: {PrimaryName}
                Game type: {GameType}
                Franchises ({FranchiseCount}): {Franchises}
                Collections ({CollectionCount}): {Collections}
                Shared IDs between both fields: {SharedIds}
                """,
                game.Id,
                FormatValue(game.Name),
                FormatGameType(game.GameType),
                game.Franchises.Count,
                FormatReferences(game.Franchises),
                game.Collections.Count,
                FormatReferences(game.Collections),
                FormatIds(GetSharedIds(game)));
        }
    }

    private void LogSummary(IReadOnlyList<IgdbGameSample> games)
    {
        var gamesWithFranchises = games.Count(game => game.Franchises.Count > 0);
        var gamesWithCollections = games.Count(game => game.Collections.Count > 0);
        var gamesWithBoth = games.Count(game =>
            game.Franchises.Count > 0 && game.Collections.Count > 0);
        var gamesWithNeither = games.Count(game =>
            game.Franchises.Count == 0 && game.Collections.Count == 0);
        var gamesWithMultipleFranchises = games.Count(game =>
            game.Franchises.Count > 1);
        var gamesWithMultipleCollections = games.Count(game =>
            game.Collections.Count > 1);
        var gamesWithSharedIds = games.Count(game => GetSharedIds(game).Length > 0);

        var franchiseReferences = games.SelectMany(game => game.Franchises).ToArray();
        var collectionReferences = games.SelectMany(game => game.Collections).ToArray();

        var franchiseIntegrity = CalculateIntegrity(games, game => game.Franchises);
        var collectionIntegrity = CalculateIntegrity(games, game => game.Collections);

        logger.LogInformation(
            """
            IGDB franchises-and-collections analysis completed:
            Requested fixed IDs: {RequestedGameCount}
            Returned unique games: {ReturnedGameCount}

            Franchise coverage:
            Games with at least one franchise: {GamesWithFranchises} ({FranchiseCoverage:F2}%)
            Games without franchise data: {GamesWithoutFranchises} ({MissingFranchiseCoverage:F2}%)
            Total franchise references: {FranchiseReferenceCount}
            Games with multiple franchises: {GamesWithMultipleFranchises}
            Franchise references with invalid ID: {InvalidFranchiseIds}
            Franchise references without name: {MissingFranchiseNames}
            Duplicate franchise IDs within a game: {DuplicateFranchiseIds}

            Collection coverage:
            Games with at least one collection: {GamesWithCollections} ({CollectionCoverage:F2}%)
            Games without collection data: {GamesWithoutCollections} ({MissingCollectionCoverage:F2}%)
            Total collection references: {CollectionReferenceCount}
            Games with multiple collections: {GamesWithMultipleCollections}
            Collection references with invalid ID: {InvalidCollectionIds}
            Collection references without name: {MissingCollectionNames}
            Duplicate collection IDs within a game: {DuplicateCollectionIds}

            Relationship between fields:
            Games with both fields: {GamesWithBoth}
            Games with franchise only: {GamesWithFranchiseOnly}
            Games with collection only: {GamesWithCollectionOnly}
            Games with neither field: {GamesWithNeither}
            Games sharing at least one numeric ID between fields: {GamesWithSharedIds}

            Coverage by game type:
            {GameTypeCoverage}

            Recurring franchises in the sample:
            {FranchiseFrequency}

            Recurring collections in the sample:
            {CollectionFrequency}
            """,
            SelectedGameIds.Count,
            games.Count,
            gamesWithFranchises,
            Percentage(gamesWithFranchises, games.Count),
            games.Count - gamesWithFranchises,
            Percentage(games.Count - gamesWithFranchises, games.Count),
            franchiseReferences.Length,
            gamesWithMultipleFranchises,
            franchiseIntegrity.InvalidIds,
            franchiseIntegrity.MissingNames,
            franchiseIntegrity.DuplicateIdsWithinGames,
            gamesWithCollections,
            Percentage(gamesWithCollections, games.Count),
            games.Count - gamesWithCollections,
            Percentage(games.Count - gamesWithCollections, games.Count),
            collectionReferences.Length,
            gamesWithMultipleCollections,
            collectionIntegrity.InvalidIds,
            collectionIntegrity.MissingNames,
            collectionIntegrity.DuplicateIdsWithinGames,
            gamesWithBoth,
            gamesWithFranchises - gamesWithBoth,
            gamesWithCollections - gamesWithBoth,
            gamesWithNeither,
            gamesWithSharedIds,
            FormatGameTypeCoverage(games),
            FormatFrequency(games, game => game.Franchises),
            FormatFrequency(games, game => game.Collections));
    }

    private static ReferenceIntegrity CalculateIntegrity(
        IReadOnlyList<IgdbGameSample> games,
        Func<IgdbGameSample, IReadOnlyList<IgdbNamedReference>> selector)
    {
        var references = games.SelectMany(selector).ToArray();
        return new ReferenceIntegrity(
            references.Count(reference => reference.Id <= 0),
            references.Count(reference => string.IsNullOrWhiteSpace(reference.Name)),
            games.Sum(game => selector(game)
                .GroupBy(reference => reference.Id)
                .Sum(group => Math.Max(0, group.Count() - 1))));
    }

    private static string FormatGameTypeCoverage(IReadOnlyList<IgdbGameSample> games) =>
        string.Join(Environment.NewLine, games
            .GroupBy(game => new
            {
                Id = game.GameType?.Id,
                Name = FormatValue(game.GameType?.Type)
            })
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key.Id)
            .Select(group =>
                $"- {group.Key.Id?.ToString() ?? "null"} | {group.Key.Name}: " +
                $"games={group.Count()}, " +
                $"with franchises={group.Count(game => game.Franchises.Count > 0)}, " +
                $"with collections={group.Count(game => game.Collections.Count > 0)}, " +
                $"with both={group.Count(game => game.Franchises.Count > 0 && game.Collections.Count > 0)}"));

    private static string FormatFrequency(
        IReadOnlyList<IgdbGameSample> games,
        Func<IgdbGameSample, IReadOnlyList<IgdbNamedReference>> selector)
    {
        var frequencies = games
            .SelectMany(game => selector(game).Select(reference => new { game.Id, Reference = reference }))
            .GroupBy(item => item.Reference.Id)
            .Select(group => new
            {
                ReferenceId = group.Key,
                Name = group.Select(item => item.Reference.Name)
                    .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)),
                GameCount = group.Select(item => item.Id).Distinct().Count()
            })
            .Where(item => item.GameCount > 1)
            .OrderByDescending(item => item.GameCount)
            .ThenBy(item => item.ReferenceId)
            .ToArray();

        return frequencies.Length == 0
            ? "None occurred in more than one sampled game."
            : string.Join(Environment.NewLine, frequencies.Select(item =>
                $"- {item.ReferenceId} | {FormatValue(item.Name)}: {item.GameCount} games"));
    }

    private static long[] GetSharedIds(IgdbGameSample game)
    {
        var collectionIds = game.Collections.Select(reference => reference.Id).ToHashSet();
        return game.Franchises
            .Select(reference => reference.Id)
            .Where(collectionIds.Contains)
            .Distinct()
            .Order()
            .ToArray();
    }

    private static string FormatReferences(IReadOnlyList<IgdbNamedReference> references) =>
        references.Count == 0
            ? "Not reported by IGDB"
            : string.Join("; ", references
                .OrderBy(reference => reference.Id)
                .Select(reference => $"{reference.Id} | {FormatValue(reference.Name)}"));

    private static string FormatGameType(IgdbGameTypeReference? gameType) =>
        gameType is null
            ? "Not reported by IGDB"
            : $"{gameType.Id} | {FormatValue(gameType.Type)}";

    private static string FormatValue(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "Not reported by IGDB" : value;

    private static string FormatIds(IEnumerable<long> ids)
    {
        var values = ids.ToArray();
        return values.Length == 0 ? "None" : string.Join(", ", values);
    }

    private static double Percentage(int value, int total) =>
        total == 0 ? 0 : value * 100d / total;

    private sealed record ReferenceIntegrity(
        int InvalidIds,
        int MissingNames,
        int DuplicateIdsWithinGames);
}