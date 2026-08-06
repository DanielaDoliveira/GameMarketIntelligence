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
                Starting IGDB involved-companies analysis:
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

            var games = await igdbClient.GetGamesByIdsAsync(
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
                "IGDB involved-companies analysis was cancelled.");
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "IGDB involved-companies analysis failed.");
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

        var returnedIds = games.Select(game => game.Id).ToHashSet();
        var selectedIds = SelectedGameIds.ToHashSet();

        var missingIds = SelectedGameIds
            .Where(gameId => !returnedIds.Contains(gameId))
            .Order()
            .ToArray();

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

    private void LogGameEvidence(IReadOnlyList<IgdbGameSample> games)
    {
        foreach (var game in games.OrderBy(game => game.Id))
        {
            logger.LogInformation(
                """
                IGDB involved-companies evidence:
                Game ID: {GameId}
                Primary name: {PrimaryName}
                Game type: {GameType}
                Involved companies ({RelationshipCount}):
                {InvolvedCompanies}
                """,
                game.Id,
                FormatValue(game.Name),
                FormatGameType(game.GameType),
                game.InvolvedCompanies.Count,
                FormatInvolvedCompanies(game.InvolvedCompanies));
        }
    }

    private void LogSummary(IReadOnlyList<IgdbGameSample> games)
    {
        var relationships = games
            .SelectMany(game => game.InvolvedCompanies.Select(relationship =>
                new GameCompanyRelationship(game, relationship)))
            .ToArray();

        var gamesWithCompanies = games.Count(
            game => game.InvolvedCompanies.Count > 0);

        var gamesWithDeveloper = games.Count(game =>
            game.InvolvedCompanies.Any(relationship => relationship.Developer));

        var gamesWithPublisher = games.Count(game =>
            game.InvolvedCompanies.Any(relationship => relationship.Publisher));

        var gamesWithPorting = games.Count(game =>
            game.InvolvedCompanies.Any(relationship => relationship.Porting));

        var gamesWithSupporting = games.Count(game =>
            game.InvolvedCompanies.Any(relationship => relationship.Supporting));

        var relationshipsWithoutCompany = relationships.Count(item =>
            item.Relationship.Company is null);

        var relationshipsWithInvalidCompanyId = relationships.Count(item =>
            item.Relationship.Company is not null &&
            item.Relationship.Company.Id <= 0);

        var relationshipsWithoutCompanyName = relationships.Count(item =>
            item.Relationship.Company is not null &&
            string.IsNullOrWhiteSpace(item.Relationship.Company.Name));

        var relationshipsWithoutRole = relationships.Count(item =>
            GetRoleCount(item.Relationship) == 0);

        var relationshipsWithMultipleRoles = relationships.Count(item =>
            GetRoleCount(item.Relationship) > 1);

        var duplicateRelationshipIds = games.Sum(game =>
            game.InvolvedCompanies
                .GroupBy(relationship => relationship.Id)
                .Sum(group => Math.Max(0, group.Count() - 1)));

        var duplicateCompanyRoleRelationships = games.Sum(game =>
            game.InvolvedCompanies
                .Where(relationship => relationship.Company is not null)
                .GroupBy(relationship => new
                {
                    relationship.Company!.Id,
                    relationship.Developer,
                    relationship.Publisher,
                    relationship.Porting,
                    relationship.Supporting
                })
                .Sum(group => Math.Max(0, group.Count() - 1)));

        var repeatedCompaniesWithinGames = games.Sum(game =>
            game.InvolvedCompanies
                .Where(relationship => relationship.Company is not null)
                .GroupBy(relationship => relationship.Company!.Id)
                .Sum(group => Math.Max(0, group.Count() - 1)));

        var gameTypeCoverage = games
            .GroupBy(game => new
            {
                Id = game.GameType?.Id,
                Name = FormatValue(game.GameType?.Type)
            })
            .Select(group => new GameTypeCoverage(
                group.Key.Id,
                group.Key.Name,
                group.Count(),
                group.Count(game => game.InvolvedCompanies.Count > 0),
                group.Count(game => game.InvolvedCompanies.Any(
                    relationship => relationship.Developer)),
                group.Count(game => game.InvolvedCompanies.Any(
                    relationship => relationship.Publisher))))
            .OrderByDescending(summary => summary.GameCount)
            .ThenBy(summary => summary.GameTypeId)
            .ToArray();

        var companyFrequency = relationships
            .Where(item => item.Relationship.Company is not null)
            .GroupBy(item => item.Relationship.Company!.Id)
            .Select(group => new CompanyFrequency(
                group.Key,
                group.Select(item => item.Relationship.Company!.Name)
                    .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)),
                group.Select(item => item.Game.Id).Distinct().Count(),
                group.Count(item => item.Relationship.Developer),
                group.Count(item => item.Relationship.Publisher),
                group.Count(item => item.Relationship.Porting),
                group.Count(item => item.Relationship.Supporting)))
            .OrderByDescending(summary => summary.GameCount)
            .ThenBy(summary => summary.CompanyId)
            .ToArray();

        logger.LogInformation(
            """
            IGDB involved-companies analysis completed:
            Requested fixed IDs: {RequestedGameCount}
            Returned unique games: {ReturnedGameCount}
            Games with at least one involved company: {GamesWithCompanies} ({CompanyCoverage:F2}%)
            Games without involved-company data: {GamesWithoutCompanies} ({MissingCompanyCoverage:F2}%)
            Total involved-company relationships: {RelationshipCount}
            Games with at least one developer: {GamesWithDeveloper} ({DeveloperCoverage:F2}%)
            Games without a developer reported by IGDB: {GamesWithoutDeveloper} ({MissingDeveloperCoverage:F2}%)
            Games with at least one publisher: {GamesWithPublisher} ({PublisherCoverage:F2}%)
            Games without a publisher reported by IGDB: {GamesWithoutPublisher} ({MissingPublisherCoverage:F2}%)
            Games with at least one porting company: {GamesWithPorting} ({PortingCoverage:F2}%)
            Games with at least one supporting company: {GamesWithSupporting} ({SupportingCoverage:F2}%)
            Developer role occurrences: {DeveloperRelationships}
            Publisher role occurrences: {PublisherRelationships}
            Porting role occurrences: {PortingRelationships}
            Supporting role occurrences: {SupportingRelationships}
            Relationships without a company object: {RelationshipsWithoutCompany}
            Relationships with a non-positive company ID: {RelationshipsWithInvalidCompanyId}
            Relationships without a company name: {RelationshipsWithoutCompanyName}
            Relationships without any role: {RelationshipsWithoutRole}
            Relationships with multiple roles: {RelationshipsWithMultipleRoles}
            Duplicate relationship-ID occurrences within the same game: {DuplicateRelationshipIds}
            Duplicate company-and-role occurrences within the same game: {DuplicateCompanyRoleRelationships}
            Repeated company occurrences within the same game: {RepeatedCompaniesWithinGames}
            Coverage by game type:
            {GameTypeCoverage}
            Company frequency across the fixed sample:
            {CompanyFrequency}
            """,
            SelectedGameIds.Count,
            games.Count,
            gamesWithCompanies,
            Percentage(gamesWithCompanies, games.Count),
            games.Count - gamesWithCompanies,
            Percentage(games.Count - gamesWithCompanies, games.Count),
            relationships.Length,
            gamesWithDeveloper,
            Percentage(gamesWithDeveloper, games.Count),
            games.Count - gamesWithDeveloper,
            Percentage(games.Count - gamesWithDeveloper, games.Count),
            gamesWithPublisher,
            Percentage(gamesWithPublisher, games.Count),
            games.Count - gamesWithPublisher,
            Percentage(games.Count - gamesWithPublisher, games.Count),
            gamesWithPorting,
            Percentage(gamesWithPorting, games.Count),
            gamesWithSupporting,
            Percentage(gamesWithSupporting, games.Count),
            relationships.Count(item => item.Relationship.Developer),
            relationships.Count(item => item.Relationship.Publisher),
            relationships.Count(item => item.Relationship.Porting),
            relationships.Count(item => item.Relationship.Supporting),
            relationshipsWithoutCompany,
            relationshipsWithInvalidCompanyId,
            relationshipsWithoutCompanyName,
            relationshipsWithoutRole,
            relationshipsWithMultipleRoles,
            duplicateRelationshipIds,
            duplicateCompanyRoleRelationships,
            repeatedCompaniesWithinGames,
            FormatGameTypeCoverage(gameTypeCoverage),
            FormatCompanyFrequency(companyFrequency));
    }

    private static int GetRoleCount(IgdbInvolvedCompanyReference relationship)
    {
        return Convert.ToInt32(relationship.Developer) +
               Convert.ToInt32(relationship.Publisher) +
               Convert.ToInt32(relationship.Porting) +
               Convert.ToInt32(relationship.Supporting);
    }

    private static string FormatInvolvedCompanies(
        IReadOnlyList<IgdbInvolvedCompanyReference> relationships)
    {
        return relationships.Count == 0
            ? "(not reported by IGDB)"
            : string.Join(
                Environment.NewLine,
                relationships
                    .OrderBy(relationship => relationship.Id)
                    .Select(relationship =>
                        $"- relationship={relationship.Id}; " +
                        $"company={FormatCompany(relationship.Company)}; " +
                        $"roles={FormatRoles(relationship)}"));
    }

    private static string FormatCompany(IgdbNamedReference? company)
    {
        return company is null
            ? "(null)"
            : $"{company.Id} - {FormatValue(company.Name)}";
    }

    private static string FormatRoles(IgdbInvolvedCompanyReference relationship)
    {
        var roles = new List<string>(4);

        if (relationship.Developer) roles.Add("developer");
        if (relationship.Publisher) roles.Add("publisher");
        if (relationship.Porting) roles.Add("porting");
        if (relationship.Supporting) roles.Add("supporting");

        return roles.Count == 0 ? "(none)" : string.Join(", ", roles);
    }

    private static string FormatGameType(IgdbGameTypeReference? gameType)
    {
        return gameType is null
            ? "(null)"
            : $"{gameType.Id} - {FormatValue(gameType.Type)}";
    }

    private static string FormatGameTypeCoverage(
        IReadOnlyList<GameTypeCoverage> summaries)
    {
        return summaries.Count == 0
            ? "(none)"
            : string.Join(
                Environment.NewLine,
                summaries.Select(summary =>
                    $"- {summary.GameTypeId?.ToString() ?? "(null)"} - " +
                    $"{summary.GameTypeName}: games={summary.GameCount}; " +
                    $"with companies={summary.GamesWithCompanies} " +
                    $"({Percentage(summary.GamesWithCompanies, summary.GameCount):F2}%); " +
                    $"with developer={summary.GamesWithDeveloper}; " +
                    $"with publisher={summary.GamesWithPublisher}"));
    }

    private static string FormatCompanyFrequency(
        IReadOnlyList<CompanyFrequency> summaries)
    {
        return summaries.Count == 0
            ? "(none)"
            : string.Join(
                Environment.NewLine,
                summaries.Select(summary =>
                    $"- {summary.CompanyId} - {FormatValue(summary.CompanyName)}: " +
                    $"games={summary.GameCount}; developer={summary.DeveloperCount}; " +
                    $"publisher={summary.PublisherCount}; porting={summary.PortingCount}; " +
                    $"supporting={summary.SupportingCount}"));
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
        return values.Length == 0 ? "(none)" : string.Join(", ", values);
    }

    private static double Percentage(int value, int total)
    {
        return total == 0 ? 0 : value * 100d / total;
    }

    private sealed record GameCompanyRelationship(
        IgdbGameSample Game,
        IgdbInvolvedCompanyReference Relationship);

    private sealed record GameTypeCoverage(
        long? GameTypeId,
        string GameTypeName,
        int GameCount,
        int GamesWithCompanies,
        int GamesWithDeveloper,
        int GamesWithPublisher);

    private sealed record CompanyFrequency(
        long CompanyId,
        string? CompanyName,
        int GameCount,
        int DeveloperCount,
        int PublisherCount,
        int PortingCount,
        int SupportingCount);
}