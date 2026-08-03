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
                "Starting IGDB bundle-composition proof of concept.");

            var tokenResponse =
                await authenticationService.GetAccessTokenAsync(stoppingToken);

            if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                throw new InvalidOperationException(
                    "Twitch returned an empty access token.");
            }

            const long bundleId = 90628;

            var games =
                await igdbClient.GetGamesIncludedInBundleAsync(
                    tokenResponse.AccessToken,
                    bundleId,
                    stoppingToken);

            logger.LogInformation(
                "IGDB returned {GameCount} records included in bundle {BundleId}.",
                games.Count,
                bundleId);

            foreach (var game in games)
            {
                logger.LogInformation(
                    """
                    Included record:
                    Id: {GameId}
                    Name: {GameName}
                    Game type: {GameType}
                    Version parent: {VersionParent}
                    Parent game: {ParentGame}
                    Bundles: {Bundles}
                    Platforms: {Platforms}
                    First release date: {FirstReleaseDate}
                    """,
                    game.Id,
                    game.Name,
                    FormatGameType(game.GameType),
                    FormatReference(game.VersionParent),
                    FormatReference(game.ParentGame),
                    FormatReferences(game.Bundles),
                    FormatReferences(game.Platforms),
                    ConvertUnixTimestamp(game.FirstReleaseDate));
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation(
                "IGDB bundle-composition proof of concept was cancelled.");
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "IGDB bundle-composition proof of concept failed.");
        }
        finally
        {
            applicationLifetime.StopApplication();
        }
    }

    private static string FormatReference(
        IgdbNamedReference? reference)
    {
        return reference is null
            ? "(null)"
            : $"{reference.Id} - {reference.Name}";
    }

    private static string FormatReferences(
        IReadOnlyList<IgdbNamedReference> references)
    {
        return references.Count == 0
            ? "(none)"
            : string.Join(
                ", ",
                references.Select(
                    reference =>
                        $"{reference.Id} - {reference.Name}"));
    }

    private static string FormatGameType(
        IgdbGameTypeReference? gameType)
    {
        return gameType is null
            ? "(null)"
            : $"{gameType.Id} - {gameType.Type}";
    }

    private static DateTimeOffset? ConvertUnixTimestamp(
        long? value)
    {
        return value.HasValue
            ? DateTimeOffset.FromUnixTimeSeconds(value.Value)
            : null;
    }
}