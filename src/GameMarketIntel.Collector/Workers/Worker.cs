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

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        try
        {
            ValidateSelectedGameIds();

            logger.LogInformation(
                """
                Starting IGDB alternative-name analysis:
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

            var games =
                await igdbClient.GetGamesAlternativeNamesSampleAsync(
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
                "IGDB alternative-name analysis was cancelled.");
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "IGDB alternative-name analysis failed.");
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

    private static void ValidateReturnedGames(
        IReadOnlyList<IgdbGameSample> games)
    {
        var duplicateIds = games
            .GroupBy(game => game.Id)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .Order()
            .ToArray();

        var returnedIds = games
            .Select(game => game.Id)
            .ToHashSet();

        var missingIds = SelectedGameIds
            .Where(gameId => !returnedIds.Contains(gameId))
            .Order()
            .ToArray();

        var selectedIds = SelectedGameIds.ToHashSet();

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

    private void LogGameEvidence(
        IReadOnlyList<IgdbGameSample> games)
    {
        foreach (var game in games.OrderBy(game => game.Id))
        {
            logger.LogInformation(
                """
                IGDB alternative-name evidence:
                Game ID: {GameId}
                Primary name: {PrimaryName}
                Version title: {VersionTitle}
                Alternative names ({AlternativeNameCount}):
                {AlternativeNames}
                Game localizations ({LocalizationCount}):
                {GameLocalizations}
                """,
                game.Id,
                FormatValue(game.Name),
                FormatValue(game.VersionTitle),
                game.AlternativeNames.Count,
                FormatAlternativeNames(game.AlternativeNames),
                game.GameLocalizations.Count,
                FormatGameLocalizations(game.GameLocalizations));
        }
    }

    private void LogSummary(
        IReadOnlyList<IgdbGameSample> games)
    {
        var gamesWithAlternativeNames = games.Count(
            game => game.AlternativeNames.Count > 0);

        var gamesWithVersionTitle = games.Count(
            game => !string.IsNullOrWhiteSpace(game.VersionTitle));

        var gamesWithLocalizations = games.Count(
            game => game.GameLocalizations.Count > 0);

        var alternativeNames = games
            .SelectMany(game => game.AlternativeNames)
            .ToArray();

        var localizations = games
            .SelectMany(game => game.GameLocalizations)
            .ToArray();

        var alternativeNamesWithComment = alternativeNames.Count(
            alternativeName =>
                !string.IsNullOrWhiteSpace(alternativeName.Comment));

        var duplicateAlternativeNamesWithinGames = games.Sum(game =>
            game.AlternativeNames
                .Where(alternativeName =>
                    !string.IsNullOrWhiteSpace(alternativeName.Name))
                .GroupBy(
                    alternativeName => NormalizeName(alternativeName.Name),
                    StringComparer.Ordinal)
                .Sum(group => Math.Max(0, group.Count() - 1)));

        var alternativeNamesEqualToPrimaryName = games.Sum(game =>
            game.AlternativeNames.Count(alternativeName =>
                NormalizeName(alternativeName.Name) ==
                NormalizeName(game.Name)));

        var crossGameNameCollisions = FindCrossGameNameCollisions(games);

        logger.LogInformation(
            """
            IGDB alternative-name analysis completed:
            Requested fixed IDs: {RequestedGameCount}
            Returned unique games: {ReturnedGameCount}
            Games with alternative names: {GamesWithAlternativeNames} ({AlternativeNameCoverage:F2}%)
            Total alternative names: {TotalAlternativeNames}
            Alternative names with comment: {AlternativeNamesWithComment} ({CommentCoverage:F2}%)
            Duplicate alternative-name occurrences within the same game: {DuplicateAlternativeNamesWithinGames}
            Alternative names equal to their primary name: {AlternativeNamesEqualToPrimaryName}
            Games with version title: {GamesWithVersionTitle} ({VersionTitleCoverage:F2}%)
            Games with localizations: {GamesWithLocalizations} ({LocalizationCoverage:F2}%)
            Total localizations: {TotalLocalizations}
            Exact normalized names associated with multiple game IDs: {CrossGameCollisionCount}
            Cross-game collision evidence:
            {CrossGameCollisions}
            """,
            SelectedGameIds.Count,
            games.Count,
            gamesWithAlternativeNames,
            Percentage(gamesWithAlternativeNames, games.Count),
            alternativeNames.Length,
            alternativeNamesWithComment,
            Percentage(alternativeNamesWithComment, alternativeNames.Length),
            duplicateAlternativeNamesWithinGames,
            alternativeNamesEqualToPrimaryName,
            gamesWithVersionTitle,
            Percentage(gamesWithVersionTitle, games.Count),
            gamesWithLocalizations,
            Percentage(gamesWithLocalizations, games.Count),
            localizations.Length,
            crossGameNameCollisions.Count,
            FormatNameCollisions(crossGameNameCollisions));
    }

    private static IReadOnlyList<NameCollision> FindCrossGameNameCollisions(
        IReadOnlyList<IgdbGameSample> games)
    {
        return games
            .SelectMany(game =>
                new[] { game.Name }
                    .Concat(game.AlternativeNames.Select(name => name.Name))
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Select(name => new
                    {
                        NormalizedName = NormalizeName(name),
                        OriginalName = name.Trim(),
                        game.Id
                    }))
            .GroupBy(item => item.NormalizedName, StringComparer.Ordinal)
            .Select(group => new NameCollision(
                group.Select(item => item.OriginalName).First(),
                group.Select(item => item.Id).Distinct().Order().ToArray()))
            .Where(collision => collision.GameIds.Count > 1)
            .OrderBy(collision => collision.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string FormatNameCollisions(
        IReadOnlyList<NameCollision> collisions)
    {
        return collisions.Count == 0
            ? "(none)"
            : string.Join(
                Environment.NewLine,
                collisions.Select(collision =>
                    $"- {collision.Name}: {FormatIds(collision.GameIds)}"));
    }

    private static string FormatAlternativeNames(
        IReadOnlyList<IgdbAlternativeNameReference> alternativeNames)
    {
        return alternativeNames.Count == 0
            ? "(none)"
            : string.Join(
                Environment.NewLine,
                alternativeNames
                    .OrderBy(alternativeName => alternativeName.Id)
                    .Select(alternativeName =>
                        $"- {alternativeName.Id}: " +
                        $"{FormatValue(alternativeName.Name)}; " +
                        $"comment={FormatValue(alternativeName.Comment)}"));
    }

    private static string FormatGameLocalizations(
        IReadOnlyList<IgdbGameLocalizationReference> localizations)
    {
        return localizations.Count == 0
            ? "(none)"
            : string.Join(
                Environment.NewLine,
                localizations
                    .OrderBy(localization => localization.Id)
                    .Select(localization =>
                        $"- {localization.Id}: " +
                        $"{FormatValue(localization.Name)}; " +
                        $"region={FormatRegion(localization.Region)}"));
    }

    private static string FormatRegion(
        IgdbRegionReference? region)
    {
        return region is null
            ? "(null)"
            : $"{region.Id} - {FormatValue(region.Name)}; " +
              $"identifier={FormatValue(region.Identifier)}; " +
              $"category={FormatValue(region.Category)}";
    }

    private static string NormalizeName(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToUpperInvariant();
    }

    private static string FormatValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "(null or whitespace)"
            : value;
    }

    private static string FormatIds(IEnumerable<long> ids)
    {
        var values = ids.Select(id => id.ToString()).ToArray();

        return values.Length == 0
            ? "(none)"
            : string.Join(", ", values);
    }

    private static double Percentage(int value, int total)
    {
        return total == 0
            ? 0
            : value * 100d / total;
    }

    private sealed record NameCollision(
        string Name,
        IReadOnlyList<long> GameIds);
}