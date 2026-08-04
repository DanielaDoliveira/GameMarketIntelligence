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
            "Starting IGDB alternative-name and localization proof of concept.");

        var tokenResponse =
            await authenticationService.GetAccessTokenAsync(stoppingToken);

        if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
        {
            throw new InvalidOperationException(
                "Twitch returned an empty access token.");
        }

        long[] gameIds =
        [
            // Pokémon: official paired products with international releases
            37382,  // Pokémon Sword
            115653, // Pokémon Shield

            // Zelda: Japanese title, romanization and historical subtitle
            1022,   // The Legend of Zelda
            1029,   // The Legend of Zelda: Ocarina of Time

            // Final Fantasy: canonical and regional-numbering collisions
            16474,  // Final Fantasy II (original Japanese entry)
            77234,  // Final Fantasy III (original Japanese entry)
            16587,  // Final Fantasy IV
            387,    // Final Fantasy II (North American FF IV)
            426,    // Final Fantasy III (North American FF VI)

            // Controls
            144542, // Kitaria Fables
            974,    // Resident Evil 4
            120     // Diablo III
        ];

        var games =
            await igdbClient.GetGamesAlternativeNamesSampleAsync(
                tokenResponse.AccessToken,
                gameIds,
                stoppingToken);

        logger.LogInformation(
            "IGDB returned {GameCount} games for the controlled name sample.",
            games.Count);

        foreach (var game in games.OrderBy(game => game.Id))
        {
            logger.LogInformation(
                """
                Game:
                Id: {GameId}
                Name: {GameName}
                Alternative names count: {AlternativeNameCount}
                Alternative names:
                {AlternativeNames}
                Game localizations count: {LocalizationCount}
                Game localizations:
                {GameLocalizations}
                """,
                game.Id,
                game.Name,
                game.AlternativeNames.Count,
                FormatAlternativeNames(game.AlternativeNames),
                game.GameLocalizations.Count,
                FormatGameLocalizations(game.GameLocalizations));
        }
    }
    catch (OperationCanceledException)
        when (stoppingToken.IsCancellationRequested)
    {
        logger.LogInformation(
            "IGDB alternative-name and localization proof of concept was cancelled.");
    }
    catch (Exception exception)
    {
        logger.LogError(
            exception,
            "IGDB alternative-name and localization proof of concept failed.");
    }
    finally
    {
        applicationLifetime.StopApplication();
    }
}

    private static string FormatSearchResults(
        IReadOnlyList<IgdbGameSample> games)
    {
        if (games.Count == 0)
        {
            return "(none)";
        }

        return string.Join(
            Environment.NewLine,
            games.Select(game =>
                $"""
                 - Id: {game.Id}
                   Name: {game.Name}
                   First release date: {ConvertUnixTimestamp(game.FirstReleaseDate)}
                   Game type: {FormatGameType(game.GameType)}
                   Platforms: {FormatReferences(game.Platforms)}
                   Version parent: {FormatReference(game.VersionParent)}
                   Parent game: {FormatReference(game.ParentGame)}
                 """));
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
                references.Select(reference =>
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

    private static string FormatReleaseDates(
        IReadOnlyList<IgdbReleaseDateReference> releaseDates)
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
        return dateFormat is null
            ? "(null)"
            : $"{dateFormat.Id} - {dateFormat.Format}";
    }

    private static string FormatReleaseRegion(
        IgdbReleaseDateRegionReference? region)
    {
        return region is null
            ? "(null)"
            : $"{region.Id} - {region.Region}";
    }

    private static string FormatReleaseStatus(
        IgdbReleaseDateStatusReference? status)
    {
        return status is null
            ? "(null)"
            : $"{status.Id} - {status.Name}";
    }

    private static string FormatAlternativeNames(
        IReadOnlyList<IgdbAlternativeNameReference> alternativeNames)
    {
        if (alternativeNames.Count == 0)
        {
            return "(none)";
        }

        return string.Join(
            Environment.NewLine,
            alternativeNames.Select(alternativeName =>
                $"""
                 - Id: {alternativeName.Id}
                   Name: {alternativeName.Name}
                   Comment: {alternativeName.Comment ?? "(null)"}
                 """));
    }

    private static string FormatGameLocalizations(
        IReadOnlyList<IgdbGameLocalizationReference> localizations)
    {
        if (localizations.Count == 0)
        {
            return "(none)";
        }

        return string.Join(
            Environment.NewLine,
            localizations.Select(localization =>
                $"""
                 - Id: {localization.Id}
                   Name: {localization.Name}
                   Region: {FormatRegion(localization.Region)}
                 """));
    }

    private static string FormatRegion(
        IgdbRegionReference? region)
    {
        if (region is null)
        {
            return "(null)";
        }

        return
            $"{region.Id} - {region.Name}; " +
            $"identifier={region.Identifier ?? "(null)"}; " +
            $"category={region.Category ?? "(null)"}";
    }
}