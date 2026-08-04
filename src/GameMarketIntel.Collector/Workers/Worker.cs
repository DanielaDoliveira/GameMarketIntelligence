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
 protected override async Task ExecuteAsync(
    CancellationToken stoppingToken)
{
    try
    {
        logger.LogInformation(
            "Starting IGDB platform-release proof of concept.");

        var tokenResponse =
            await authenticationService.GetAccessTokenAsync(stoppingToken);

        if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
        {
            throw new InvalidOperationException(
                "Twitch returned an empty access token.");
        }

        long[] gameIds =
        [
            144542, // Kitaria Fables
            974,    // Resident Evil 4 (original)
            120     // Diablo III
        ];

        var games = await igdbClient.GetGamesByIdsAsync(
            tokenResponse.AccessToken,
            gameIds,
            stoppingToken);

        logger.LogInformation(
            "IGDB returned {GameCount} games for the controlled release sample.",
            games.Count);

        foreach (var game in games)
        {
            logger.LogInformation(
                """
                Game:
                Id: {GameId}
                Name: {GameName}
                First release date: {FirstReleaseDate}
                Game type: {GameType}
                Version parent: {VersionParent}
                Parent game: {ParentGame}
                Platforms: {Platforms}
                Release dates count: {ReleaseDateCount}
                Release dates:
                {ReleaseDates}
                """,
                game.Id,
                game.Name,
                ConvertUnixTimestamp(game.FirstReleaseDate),
                FormatGameType(game.GameType),
                FormatReference(game.VersionParent),
                FormatReference(game.ParentGame),
                FormatReferences(game.Platforms),
                game.ReleaseDates.Count,
                FormatReleaseDates(game.ReleaseDates));
        }
    }
    catch (OperationCanceledException)
        when (stoppingToken.IsCancellationRequested)
    {
        logger.LogInformation(
            "IGDB platform-release proof of concept was cancelled.");
    }
    catch (Exception exception)
    {
        logger.LogError(
            exception,
            "IGDB platform-release proof of concept failed.");
    }
    finally
    {
        applicationLifetime.StopApplication();
    }
}
    private static string FormatReference(IgdbNamedReference? reference)
    {
        return reference is null ? "(null)" : $"{reference.Id} - {reference.Name}";
    }

    private static string FormatReferences(
        IReadOnlyList<IgdbNamedReference> references)
    {
        return references.Count == 0
            ? "(none)" : string.Join(", ", references.Select(reference => $"{reference.Id} - {reference.Name}"));
    }

    private static string FormatGameType(IgdbGameTypeReference? gameType)
    {
        return gameType is null ? "(null)" : $"{gameType.Id} - {gameType.Type}";
    }

    private static DateTimeOffset? ConvertUnixTimestamp(long? value)
    {
        return value.HasValue
            ? DateTimeOffset.FromUnixTimeSeconds(value.Value)
            : null;
    }
    private static string FormatReleaseDates(IReadOnlyList<IgdbReleaseDateReference> releaseDates)
    {
        if (releaseDates.Count == 0)
        {
            return "(none)";
        }

        return string.Join(
            Environment.NewLine,
            releaseDates.Select(releaseDate =>
                $"""
                 - Id: {releaseDate.Id}
                   Date: {ConvertUnixTimestamp(releaseDate.Date)}
                   Human: {releaseDate.Human ?? "(null)"}
                   Components: day={releaseDate.Day?.ToString() ?? "(null)"}, month={releaseDate.Month?.ToString() ?? "(null)"}, year={releaseDate.Year?.ToString() ?? "(null)"}
                   Format: {FormatDateFormat(releaseDate.DateFormat)}
                   Platform: {FormatReference(releaseDate.Platform)}
                   Region: {FormatReleaseRegion(releaseDate.ReleaseRegion)}
                   Status: {FormatReleaseStatus(releaseDate.Status)}
                 """));
    }
    private static string FormatDateFormat(
        IgdbDateFormatReference? dateFormat)
    {
        return dateFormat is null ? "(null)" : $"{dateFormat.Id} - {dateFormat.Format}";
    }

    private static string FormatReleaseRegion(IgdbReleaseDateRegionReference? region)
    {
        return region is null ? "(null)" : $"{region.Id} - {region.Region}";
    }

    private static string FormatReleaseStatus(IgdbReleaseDateStatusReference? status)
    {
        return status is null ? "(null)" : $"{status.Id} - {status.Name}";
    }
}