using GameMarketIntel.Collector.Igdb.Authentication;
using GameMarketIntel.Collector.Igdb.Client;
using GameMarketIntel.Collector.Igdb.Poc;
using Microsoft.Extensions.Options;

namespace GameMarketIntel.Collector.Workers;

public sealed class Worker(
    IIgdbAuthenticationService authenticationService,
    IIgdbClient igdbClient,
    IOptions<IgdbPocOptions> options,
    IHostApplicationLifetime applicationLifetime,
    ILogger<Worker> logger)
    : BackgroundService
{
    private readonly IgdbPocOptions _options = options.Value;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation(
                "Starting IGDB games sample proof of concept.");

            var tokenResponse =
                await authenticationService.GetAccessTokenAsync(
                    stoppingToken);

            if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                throw new InvalidOperationException(
                    "Twitch returned an empty access token.");
            }

            var games = await igdbClient.GetGamesSampleAsync(
                tokenResponse.AccessToken,
                _options.SampleSize,
                stoppingToken);

            logger.LogInformation(
                "IGDB returned {GameCount} games.",
                games.Count);

            foreach (var game in games)
            {
                logger.LogInformation(
                    """
                    Game:
                    Id: {GameId}
                    Name: {GameName}
                    First release date: {FirstReleaseDate}
                    Updated at: {UpdatedAt}
                    """,
                    game.Id,
                    game.Name,
                    ConvertUnixTimestamp(game.FirstReleaseDate),
                    ConvertUnixTimestamp(game.UpdatedAt));
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation(
                "IGDB games sample proof of concept was cancelled.");
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "IGDB games sample proof of concept failed.");
        }
        finally
        {
            applicationLifetime.StopApplication();
        }
    }

    private static DateTimeOffset? ConvertUnixTimestamp(long? value)
    {
        return value.HasValue
            ? DateTimeOffset.FromUnixTimeSeconds(value.Value)
            : null;
    }
}