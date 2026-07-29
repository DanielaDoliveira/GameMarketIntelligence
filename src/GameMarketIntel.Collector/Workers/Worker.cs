using GameMarketIntel.Collector.Igdb.Authentication;
using GameMarketIntel.Collector.Igdb.Client;
using GameMarketIntel.Collector.Igdb.Contracts;
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
            logger.LogInformation("Starting IGDB games sample proof of concept.");

            var tokenResponse = await authenticationService.GetAccessTokenAsync(stoppingToken);

            if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
                throw new InvalidOperationException("Twitch returned an empty access token.");
            

            var gameIds = new long[]
            {
                144542,
                166686,
                340742
            };

            var games = await igdbClient.GetGamesByIdsAsync(
                tokenResponse.AccessToken,
                gameIds,
                stoppingToken);
            
            
            logger.LogInformation("Starting controlled IGDB games proof of concept.");
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
                    Game type: {GameType}
                    Game status: {GameStatus}
                    Version parent: {VersionParent}
                    Parent game: {ParentGame}
                    Platforms: {Platforms}
                    Genres: {Genres}
                    Themes: {Themes}
                    Keywords: {Keywords}
                    First release date: {FirstReleaseDate}
                    Updated at: {UpdatedAt}
                    """,
                    game.Id,
                    game.Name,
                    FormatGameType(game.GameType),
                    FormatGameStatus(game.GameStatus),
                    FormatReference(game.VersionParent),
                    FormatReference(game.ParentGame),
                    FormatReferences(game.Platforms),
                    FormatReferences(game.Genres),
                    FormatReferences(game.Themes),
                    FormatReferences(game.Keywords),
                    ConvertUnixTimestamp(game.FirstReleaseDate),
                    ConvertUnixTimestamp(game.UpdatedAt));
            }
        }
        catch (OperationCanceledException)when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("IGDB games sample proof of concept was cancelled.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "IGDB games sample proof of concept failed.");
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

    private static string FormatReferences(IReadOnlyList<IgdbNamedReference> references)
    {
        return references.Count == 0 ? "(none)" : string.Join(", ", references.Select(reference => $"{reference.Id} - {reference.Name}"));
    }

    private static DateTimeOffset? ConvertUnixTimestamp(long? value)
    {
        return value.HasValue ? DateTimeOffset.FromUnixTimeSeconds(value.Value) : null;
    }
    private static string FormatGameType(IgdbGameTypeReference? gameType)
    {
        return gameType is null ? "(null)" : $"{gameType.Id} - {gameType.Type}";
    }

    private static string FormatGameStatus(IgdbGameStatusReference? gameStatus)
    {
        return gameStatus is null ? "(null)" : $"{gameStatus.Id} - {gameStatus.Status}";
    }
}