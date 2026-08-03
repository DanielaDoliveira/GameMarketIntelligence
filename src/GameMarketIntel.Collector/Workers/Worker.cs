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
                "Starting IGDB commercial-eligibility proof of concept.");

            var tokenResponse =
                await authenticationService.GetAccessTokenAsync(stoppingToken);

            if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                throw new InvalidOperationException(
                    "Twitch returned an empty access token.");
            }

            var gameIds = new long[]
            {
                294763, // Mario Party: Love Land
                6739,   // Black Mesa
                132181  // Resident Evil 4 Remake
            };

            var games = await igdbClient.GetGamesByIdsAsync(
                tokenResponse.AccessToken,
                gameIds,
                stoppingToken);

            logger.LogInformation(
                "IGDB returned {GameCount} controlled records.",
                games.Count);

            foreach (var game in games)
            {
                logger.LogInformation(
                    """
                    Game:
                    Id: {GameId}
                    Name: {GameName}
                    Game type: {GameType}
                    Parent game: {ParentGame}
                    Companies: {Companies}
                    External games: {ExternalGames}
                    Websites: {Websites}
                    """,
                    game.Id,
                    game.Name,
                    FormatGameType(game.GameType),
                    FormatReference(game.ParentGame),
                    FormatInvolvedCompanies(game.InvolvedCompanies),
                    FormatExternalGames(game.ExternalGames),
                    FormatWebsites(game.Websites));
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation(
                "IGDB commercial-eligibility proof of concept was cancelled.");
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "IGDB commercial-eligibility proof of concept failed.");
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

    private static string FormatGameType(
        IgdbGameTypeReference? gameType)
    {
        return gameType is null
            ? "(null)"
            : $"{gameType.Id} - {gameType.Type}";
    }

    private static string FormatInvolvedCompanies(
        IReadOnlyList<IgdbInvolvedCompanyReference> companies)
    {
        if (companies.Count == 0)
        {
            return "(none)";
        }

        return string.Join(
            ", ",
            companies.Select(company =>
                $"{FormatReference(company.Company)} " +
                $"[Developer: {company.Developer}, " +
                $"Publisher: {company.Publisher}, " +
                $"Porting: {company.Porting}, " +
                $"Supporting: {company.Supporting}]"));
    }

    private static string FormatExternalGames(
        IReadOnlyList<IgdbExternalGameReference> externalGames)
    {
        if (externalGames.Count == 0)
        {
            return "(none)";
        }

        return string.Join(
            ", ",
            externalGames.Select(externalGame =>
                $"{externalGame.Source?.Name ?? "(unknown source)"} " +
                $"- {externalGame.ExternalId ?? "(no id)"} " +
                $"- {externalGame.Url ?? "(no url)"}"));
    }

    private static string FormatWebsites(
        IReadOnlyList<IgdbWebsiteReference> websites)
    {
        if (websites.Count == 0)
        {
            return "(none)";
        }

        return string.Join(
            ", ",
            websites.Select(website =>
                $"{website.Type?.Type ?? "(unknown type)"} " +
                $"- Trusted: {website.Trusted} " +
                $"- {website.Url}"));
    }
}