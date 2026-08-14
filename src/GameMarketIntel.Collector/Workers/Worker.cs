using System.Diagnostics;
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
    private const string SampleCutoff = "2026-08-04T00:00:00Z";
    private static readonly TimeSpan MinimumRequestStartInterval =
        TimeSpan.FromMilliseconds(275);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var cutoff = DateTimeOffset.Parse(SampleCutoff).ToUnixTimeSeconds();

            logger.LogInformation(
                """
                Starting controlled IGDB operational analysis:
                Cutoff: {Cutoff}
                Minimum interval between request starts: {IntervalMs} ms
                The test will not intentionally trigger HTTP 429.
                """,
                SampleCutoff,
                MinimumRequestStartInterval.TotalMilliseconds);

            var token = await authenticationService.GetAccessTokenAsync(stoppingToken);
            if (string.IsNullOrWhiteSpace(token.AccessToken))
            {
                throw new InvalidOperationException("Twitch returned an empty access token.");
            }

            var total = await igdbClient.CountReleasedGamesAsync(
                token.AccessToken,
                cutoff,
                stoppingToken);

            if (total == 0)
            {
                throw new InvalidOperationException(
                    "IGDB returned no released games for the operational test.");
            }

            var offsets = BuildOffsets(total);
            var observations = new List<PageObservation>(offsets.Length);
            DateTimeOffset? previousRequestStart = null;

            foreach (var offset in offsets)
            {
                await DelayUntilNextAllowedStartAsync(
                    previousRequestStart,
                    stoppingToken);

                var requestStart = DateTimeOffset.UtcNow;
                previousRequestStart = requestStart;
                var stopwatch = Stopwatch.StartNew();

                var games = await igdbClient.GetReleasedGameAtOffsetAsync(
                    token.AccessToken,
                    cutoff,
                    offset,
                    stoppingToken);

                stopwatch.Stop();
                var game = games.SingleOrDefault();
                observations.Add(new PageObservation(
                    offset,
                    game?.Id,
                    game?.Name,
                    requestStart,
                    stopwatch.Elapsed));

                logger.LogInformation(
                    "Offset {Offset}: records={RecordCount}; game ID={GameId}; " +
                    "title={Title}; elapsed={ElapsedMs} ms",
                    offset,
                    games.Count,
                    game?.Id,
                    game?.Name ?? "not returned",
                    stopwatch.Elapsed.TotalMilliseconds);
            }

            await DelayUntilNextAllowedStartAsync(previousRequestStart, stoppingToken);
            var repeatedOffset = offsets.Contains(500) ? 500 : offsets[0];
            var repeatedPage = await igdbClient.GetReleasedGameAtOffsetAsync(
                token.AccessToken,
                cutoff,
                repeatedOffset,
                stoppingToken);
            var repeatedGame = repeatedPage.SingleOrDefault();
            var originalGame = observations.Single(item => item.Offset == repeatedOffset);

            var emptyPages = observations.Count(item => item.GameId is null);
            var duplicateIds = observations
                .Where(item => item.GameId.HasValue)
                .GroupBy(item => item.GameId)
                .Count(group => group.Count() > 1);
            var minimumObservedStartInterval = observations.Count < 2
                ? TimeSpan.Zero
                : observations
                    .Zip(observations.Skip(1), (previous, current) =>
                        current.StartedAt - previous.StartedAt)
                    .Min();
            var stableRepeat = repeatedGame?.Id == originalGame.GameId;

            logger.LogInformation(
                """
                IGDB controlled operational summary:
                Eligible record count: {Total}
                Offsets tested: {Offsets}
                Successful offset reads: {Successful}/{Attempted}
                Empty offset reads: {EmptyPages}
                Duplicate IDs across distinct offsets: {DuplicateIds}
                Minimum observed interval between request starts: {MinimumIntervalMs} ms
                Repeated offset: {RepeatedOffset}
                Original repeated-offset game ID: {OriginalId}
                Second repeated-offset game ID: {RepeatedId}
                Stable repeated-offset result: {StableRepeat}

                Interpretation:
                - pagination passes when every valid offset returns one record,
                  distinct offsets return distinct IDs and the repeated offset is stable;
                - pacing passes when the minimum observed start interval is at least
                  {ConfiguredIntervalMs} ms;
                - HTTP 429/retry behavior remains a simulated-test concern and is not
                  provoked against the live IGDB API.
                """,
                total,
                string.Join(", ", offsets),
                observations.Count - emptyPages,
                observations.Count,
                emptyPages,
                duplicateIds,
                minimumObservedStartInterval.TotalMilliseconds,
                repeatedOffset,
                originalGame.GameId,
                repeatedGame?.Id,
                stableRepeat,
                MinimumRequestStartInterval.TotalMilliseconds);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("IGDB operational analysis was cancelled.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "IGDB operational analysis failed.");
        }
        finally
        {
            applicationLifetime.StopApplication();
        }
    }

    private static int[] BuildOffsets(int total)
    {
        var candidates = new[]
        {
            0,
            1,
            2,
            499,
            500,
            501,
            total / 2,
            total - 1
        };

        return candidates
            .Where(offset => offset >= 0 && offset < total)
            .Distinct()
            .Order()
            .ToArray();
    }

    private static async Task DelayUntilNextAllowedStartAsync(
        DateTimeOffset? previousRequestStart,
        CancellationToken cancellationToken)
    {
        if (!previousRequestStart.HasValue)
        {
            return;
        }

        var elapsed = DateTimeOffset.UtcNow - previousRequestStart.Value;
        var remaining = MinimumRequestStartInterval - elapsed;

        if (remaining > TimeSpan.Zero)
        {
            await Task.Delay(remaining, cancellationToken);
        }
    }

    private sealed record PageObservation(
        int Offset,
        long? GameId,
        string? GameName,
        DateTimeOffset StartedAt,
        TimeSpan Elapsed);
}
