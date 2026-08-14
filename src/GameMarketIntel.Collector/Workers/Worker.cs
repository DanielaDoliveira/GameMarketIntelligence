using GameMarketIntel.Collector.Igdb.Authentication;
using GameMarketIntel.Collector.Igdb.Client;
using GameMarketIntel.Collector.Igdb.Contracts;

namespace GameMarketIntel.Collector.Workers;

public sealed class Worker(
    IIgdbAuthenticationService authenticationService,
    IIgdbClient igdbClient,
    IHostApplicationLifetime applicationLifetime,
    ILogger<Worker> logger) : BackgroundService
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
                Starting IGDB consolidated coverage-and-nullability analysis:
                Frozen sample size: {SampleCount}
                Sample seed: {SampleSeed}
                Sample cutoff: {SampleCutoff}
                """,
                SampleGameIds.Length,
                SampleSeed,
                SampleCutoff);

            var token = await authenticationService.GetAccessTokenAsync(stoppingToken);
            if (string.IsNullOrWhiteSpace(token.AccessToken))
            {
                throw new InvalidOperationException("Twitch returned an empty access token.");
            }

            var games = await igdbClient.GetGamesByIdsAsync(
                token.AccessToken,
                SampleGameIds,
                stoppingToken);

            LogResults(games);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("IGDB coverage-and-nullability analysis was cancelled.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "IGDB coverage-and-nullability analysis failed.");
        }
        finally
        {
            applicationLifetime.StopApplication();
        }
    }

    private void LogResults(IReadOnlyList<IgdbGameSample> games)
    {
        var duplicateReturnedIds = games
            .GroupBy(game => game.Id)
            .Count(group => group.Count() > 1);
        var expectedIds = SampleGameIds.ToHashSet();
        var unexpectedIds = games.Count(game => !expectedIds.Contains(game.Id));
        var gamesById = games
            .GroupBy(game => game.Id)
            .ToDictionary(group => group.Key, group => group.First());
        var returned = SampleGameIds
            .Where(gamesById.ContainsKey)
            .Select(id => gamesById[id])
            .ToArray();

        var rows = new[]
        {
            Row("name", returned, game => !string.IsNullOrWhiteSpace(game.Name)),
            Row("summary", returned, game => !string.IsNullOrWhiteSpace(game.Summary)),
            Row("first_release_date", returned, game => game.FirstReleaseDate.HasValue),
            Row("updated_at", returned, game => game.UpdatedAt.HasValue),
            Row("game_type", returned, game => game.GameType is not null),
            Row("game_status", returned, game => game.GameStatus is not null),
            Row("parent_game", returned, game => game.ParentGame is not null),
            Row("version_parent", returned, game => game.VersionParent is not null),
            Row("platforms", returned, game => game.Platforms.Count > 0),
            Row("genres", returned, game => game.Genres.Count > 0),
            Row("themes", returned, game => game.Themes.Count > 0),
            Row("keywords", returned, game => game.Keywords.Count > 0),
            Row("involved_companies", returned, game => game.InvolvedCompanies.Count > 0),
            Row("collections", returned, game => game.Collections.Count > 0),
            Row("franchises", returned, game => game.Franchises.Count > 0),
            Row("release_dates", returned, game => game.ReleaseDates.Count > 0),
            Row("external_games", returned, game => game.ExternalGames.Count > 0),
            Row("websites", returned, game => game.Websites.Count > 0)
        };

        var matrix = string.Join(Environment.NewLine, rows.Select(row =>
            $"- {row.Field}: present={row.Present}/{returned.Length} " +
            $"({Percentage(row.Present, returned.Length)}); " +
            $"absent={row.Absent}/{returned.Length} " +
            $"({Percentage(row.Absent, returned.Length)})"));

        logger.LogInformation(
            """
            IGDB consolidated coverage-and-nullability summary:
            Frozen sample size: {SampleCount}
            Records returned: {ReturnedCount}
            Missing expected records: {MissingCount}
            Unexpected records: {UnexpectedCount}
            Duplicate returned IDs: {DuplicateCount}

            Field presence matrix:
            {Matrix}

            Relationship combinations:
            Records with parent_game only: {ParentOnlyCount}
            Records with version_parent only: {VersionOnlyCount}
            Records with both relationships: {BothRelationshipCount}
            Records with neither relationship: {NeitherRelationshipCount}

            Release consistency indicators:
            Records with platforms but no release_dates: {PlatformsWithoutDatesCount}
            Records with release_dates but no platforms: {DatesWithoutPlatformsCount}
            Records with first_release_date but no release_dates: {FirstWithoutDetailsCount}
            Records with release_dates but no first_release_date: {DetailsWithoutFirstCount}

            Source-link indicators:
            Records with external_games or websites: {AnySourceLinkCount}
            Records with neither external_games nor websites: {NoSourceLinkCount}
            """,
            SampleGameIds.Length,
            returned.Length,
            SampleGameIds.Length - returned.Length,
            unexpectedIds,
            duplicateReturnedIds,
            matrix,
            returned.Count(game => game.ParentGame is not null && game.VersionParent is null),
            returned.Count(game => game.ParentGame is null && game.VersionParent is not null),
            returned.Count(game => game.ParentGame is not null && game.VersionParent is not null),
            returned.Count(game => game.ParentGame is null && game.VersionParent is null),
            returned.Count(game => game.Platforms.Count > 0 && game.ReleaseDates.Count == 0),
            returned.Count(game => game.ReleaseDates.Count > 0 && game.Platforms.Count == 0),
            returned.Count(game => game.FirstReleaseDate.HasValue && game.ReleaseDates.Count == 0),
            returned.Count(game => game.ReleaseDates.Count > 0 && !game.FirstReleaseDate.HasValue),
            returned.Count(game => game.ExternalGames.Count > 0 || game.Websites.Count > 0),
            returned.Count(game => game.ExternalGames.Count == 0 && game.Websites.Count == 0));

        var missingLines = rows
            .Where(row => row.Absent > 0)
            .Select(row => $"- {row.Field}: {row.Absent} absent");

        logger.LogInformation(
            "IGDB nullable-or-absent fields in the fixed sample:{NewLine}{Fields}",
            Environment.NewLine,
            string.Join(Environment.NewLine, missingLines));

        logger.LogInformation("IGDB coverage-and-nullability analysis completed.");
    }

    private static CoverageRow Row(
        string field,
        IReadOnlyCollection<IgdbGameSample> games,
        Func<IgdbGameSample, bool> isPresent)
    {
        var present = games.Count(isPresent);
        return new CoverageRow(field, present, games.Count - present);
    }

    private static string Percentage(int numerator, int denominator) =>
        denominator == 0 ? "0.00%" : $"{(double)numerator / denominator:P2}";

    private sealed record CoverageRow(string Field, int Present, int Absent);
}
