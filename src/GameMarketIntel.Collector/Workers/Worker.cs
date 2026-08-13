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
                "Starting IGDB cover-and-screenshot analysis: sample={SampleCount}, seed={Seed}, cutoff={Cutoff}",
                SampleGameIds.Length, SampleSeed, SampleCutoff);

            var token = await authenticationService.GetAccessTokenAsync(stoppingToken);
            if (string.IsNullOrWhiteSpace(token.AccessToken))
            {
                throw new InvalidOperationException("Twitch returned an empty access token.");
            }

            var games = await igdbClient.GetGamesByIdsAsync(
                token.AccessToken, SampleGameIds, stoppingToken);
            LogResults(games);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("IGDB cover-and-screenshot analysis was cancelled.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "IGDB cover-and-screenshot analysis failed.");
        }
        finally
        {
            applicationLifetime.StopApplication();
        }
    }

    private void LogResults(IReadOnlyList<IgdbGameSample> games)
    {
        var byId = games.ToDictionary(game => game.Id);
        var returned = SampleGameIds
            .Where(byId.ContainsKey)
            .Select(id => byId[id])
            .ToArray();

        var lines = SampleGameIds.Select(id => byId.TryGetValue(id, out var game)
            ? FormatGame(game)
            : $"- ID={id}; record=NOT RETURNED");

        logger.LogInformation("IGDB image results:{NewLine}{Results}",
            Environment.NewLine, string.Join(Environment.NewLine, lines));

        LogCoverSummary(returned);
        LogScreenshotSummary(returned);
        logger.LogInformation("IGDB cover-and-screenshot analysis completed.");
    }

    private void LogCoverSummary(IReadOnlyList<IgdbGameSample> games)
    {
        var images = games.Where(g => g.Cover is not null).Select(g => g.Cover!).ToArray();

        logger.LogInformation(
            """
            IGDB cover summary:
            Frozen sample size: {SampleCount}
            Records returned: {ReturnedCount}
            Missing records: {MissingCount}
            Records with a cover: {PresentCount}
            Records without a cover: {AbsentCount}
            Cover presence coverage: {Coverage}
            Invalid record IDs: {InvalidIdCount}
            Blank image IDs: {BlankImageIdCount}
            Blank URLs: {BlankUrlCount}
            Non-positive dimensions: {InvalidDimensionsCount}
            Duplicate image IDs across games: {DuplicateImageIdCount}
            Conflicting metadata for the same image ID: {ConflictCount}
            """,
            SampleGameIds.Length, games.Count, SampleGameIds.Length - games.Count,
            images.Length, games.Count - images.Length,
            Percentage(images.Length, games.Count),
            images.Count(i => i.Id <= 0),
            images.Count(i => string.IsNullOrWhiteSpace(i.ImageId)),
            images.Count(i => string.IsNullOrWhiteSpace(i.Url)),
            images.Count(i => i.Width <= 0 || i.Height <= 0),
            DuplicateImageIds(images), ConflictingMetadata(images));
    }

    private void LogScreenshotSummary(IReadOnlyList<IgdbGameSample> games)
    {
        var populated = games.Where(g => g.Screenshots.Count > 0).ToArray();
        var images = games.SelectMany(g => g.Screenshots).ToArray();

        logger.LogInformation(
            """
            IGDB screenshot summary:
            Frozen sample size: {SampleCount}
            Records returned: {ReturnedCount}
            Missing records: {MissingCount}
            Records with screenshots: {PresentCount}
            Records without screenshots: {AbsentCount}
            Screenshot presence coverage: {Coverage}
            Records with exactly one screenshot: {ExactlyOneCount}
            Records with multiple screenshots: {MultipleCount}
            Total screenshots: {TotalCount}
            Minimum per populated record: {MinimumCount}
            Maximum per populated record: {MaximumCount}
            Average per populated record: {AverageCount:F2}
            Invalid record IDs: {InvalidIdCount}
            Blank image IDs: {BlankImageIdCount}
            Blank URLs: {BlankUrlCount}
            Non-positive dimensions: {InvalidDimensionsCount}
            Records with duplicate image IDs: {RecordsWithDuplicates}
            Duplicate image IDs across games: {DuplicatesAcrossGames}
            Conflicting metadata for the same image ID: {ConflictCount}
            """,
            SampleGameIds.Length, games.Count, SampleGameIds.Length - games.Count,
            populated.Length, games.Count - populated.Length,
            Percentage(populated.Length, games.Count),
            games.Count(g => g.Screenshots.Count == 1),
            games.Count(g => g.Screenshots.Count > 1), images.Length,
            populated.Length == 0 ? 0 : populated.Min(g => g.Screenshots.Count),
            populated.Length == 0 ? 0 : populated.Max(g => g.Screenshots.Count),
            populated.Length == 0 ? 0 : populated.Average(g => g.Screenshots.Count),
            images.Count(i => i.Id <= 0),
            images.Count(i => string.IsNullOrWhiteSpace(i.ImageId)),
            images.Count(i => string.IsNullOrWhiteSpace(i.Url)),
            images.Count(i => i.Width <= 0 || i.Height <= 0),
            games.Count(g => DuplicateImageIds(g.Screenshots) > 0),
            DuplicateImageIds(images), ConflictingMetadata(images));
    }

    private static string FormatGame(IgdbGameSample game) =>
        $"- ID={game.Id}; title={Value(game.Name)}; " +
        $"cover={(game.Cover is null ? "not reported" : FormatImage(game.Cover))}; " +
        $"screenshot count={game.Screenshots.Count}; screenshots=" +
        (game.Screenshots.Count == 0
            ? "not reported"
            : string.Join(", ", game.Screenshots.OrderBy(i => i.Id).Select(FormatImage)));

    private static string FormatImage(IgdbImageReference image) =>
        $"{image.Id}|image_id={Value(image.ImageId)}|{image.Width}x{image.Height}|url={Value(image.Url)}";

    private static int DuplicateImageIds(IEnumerable<IgdbImageReference> images) =>
        images.Where(i => !string.IsNullOrWhiteSpace(i.ImageId))
            .GroupBy(i => i.ImageId.Trim(), StringComparer.OrdinalIgnoreCase)
            .Count(group => group.Count() > 1);

    private static int ConflictingMetadata(IEnumerable<IgdbImageReference> images) =>
        images.Where(i => !string.IsNullOrWhiteSpace(i.ImageId))
            .GroupBy(i => i.ImageId.Trim(), StringComparer.OrdinalIgnoreCase)
            .Count(group => group.Select(i => (i.Url?.Trim(), i.Width, i.Height)).Distinct().Count() > 1);

    private static string Percentage(int numerator, int denominator) =>
        denominator == 0 ? "0.00%" : $"{(double)numerator / denominator:P2}";

    private static string Value(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "not reported" : value;
}
