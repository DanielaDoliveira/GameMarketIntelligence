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
    private static readonly IReadOnlyList<ExpectedCollectionCase> ExpectedCases =
    [
        new("The Legend of Zelda", 1029, "The Legend of Zelda: Ocarina of Time", 106, "The Legend of Zelda"),
        new("The Legend of Zelda", 1036, "The Legend of Zelda: Twilight Princess", 106, "The Legend of Zelda"),
        new("The Legend of Zelda", 119388, "The Legend of Zelda: Tears of the Kingdom", 106, "The Legend of Zelda"),

        new("Mario", 26758, "Super Mario Odyssey", 240, "Super Mario"),
        new("Mario", 2350, "Mario Kart 8", 449, "Mario Kart"),
        new("Mario", 1077, "Super Mario Galaxy", 240, "Super Mario"),

        new("Pokémon", 1561, "Pokémon Red Version", 314, "Pokémon"),
        new("Pokémon", 37382, "Pokémon Sword", 314, "Pokémon"),
        new("Pokémon", 144054, "Pokémon Legends: Arceus", 314, "Pokémon"),

        new("Kingdom Hearts", 393742, "Kingdom Hearts", 272, "Kingdom Hearts"),
        new("Kingdom Hearts", 1221, "Kingdom Hearts II", 272, "Kingdom Hearts"),
        new("Kingdom Hearts", 2933, "Kingdom Hearts III", 272, "Kingdom Hearts"),

        new("Final Fantasy", 393025, "Final Fantasy VII", 39, "Final Fantasy"),
        new("Final Fantasy", 418, "Final Fantasy X", 39, "Final Fantasy"),
        new("Final Fantasy", 31551, "Final Fantasy XVI", 39, "Final Fantasy"),

        new("Hollow Knight", 14593, "Hollow Knight", 5702, "Hollow Knight"),
        new("Hollow Knight", 115289, "Hollow Knight: Silksong", 5702, "Hollow Knight"),

        new("Animal Crossing", 2655, "Animal Crossing", 521, "Animal Crossing"),
        new("Animal Crossing", 2687, "Animal Crossing: New Leaf", 521, "Animal Crossing"),
        new("Animal Crossing", 109462, "Animal Crossing: New Horizons", 521, "Animal Crossing"),

        new("Splatoon", 7335, "Splatoon", 2584, "Splatoon"),
        new("Splatoon", 26761, "Splatoon 2", 2584, "Splatoon"),
        new("Splatoon", 143613, "Splatoon 3", 2584, "Splatoon"),

        new("Grand Theft Auto", 730, "Grand Theft Auto III", 847, "Grand Theft Auto"),
        new("Grand Theft Auto", 732, "Grand Theft Auto: San Andreas", 847, "Grand Theft Auto"),
        new("Grand Theft Auto", 1020, "Grand Theft Auto V", 847, "Grand Theft Auto")
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation(
                """
                Starting IGDB targeted collection-coverage analysis:
                Reference cases: {CaseCount}
                Series: {SeriesCount}
                """,
                ExpectedCases.Count,
                ExpectedCases.Select(item => item.Series).Distinct().Count());

            var tokenResponse =
                await authenticationService.GetAccessTokenAsync(stoppingToken);

            if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                throw new InvalidOperationException(
                    "Twitch returned an empty access token.");
            }

            var games = await igdbClient.GetGamesByIdsAsync(
                tokenResponse.AccessToken,
                ExpectedCases.Select(item => item.GameId).ToArray(),
                stoppingToken);

            var gamesById = games.ToDictionary(game => game.Id);
            var results = ExpectedCases
                .Select(expected => Evaluate(expected, gamesById))
                .ToArray();

            logger.LogInformation(
                """
                IGDB targeted collection-coverage results:
                {Results}
                """,
                FormatResults(results));

            logger.LogInformation(
                """
                IGDB targeted collection-coverage summary:
                Reference cases: {ReferenceCases}
                Records returned: {ReturnedRecords}
                Records with one or more collections: {WithCollections}
                Records containing the expected collection: {CorrectMatches}
                Missing records: {MissingRecords}
                Missing collections: {MissingCollections}
                Unexpected collections: {UnexpectedCollections}
                Presence coverage among all reference cases: {PresenceCoverage:F2}%
                Correct coverage among all reference cases: {CorrectCoverage:F2}%
                Correctness among records with collections: {CorrectnessWhenPresent:F2}%

                Coverage by series:
                {SeriesSummary}
                """,
                results.Length,
                results.Count(result => result.Game is not null),
                results.Count(result => result.HasCollection),
                results.Count(result => result.IsExpectedCollectionPresent),
                results.Count(result => result.Status == CoverageStatus.MissingRecord),
                results.Count(result => result.Status == CoverageStatus.MissingCollection),
                results.Count(result => result.Status == CoverageStatus.UnexpectedCollection),
                Percentage(results.Count(result => result.HasCollection), results.Length),
                Percentage(results.Count(result => result.IsExpectedCollectionPresent), results.Length),
                Percentage(
                    results.Count(result => result.IsExpectedCollectionPresent),
                    results.Count(result => result.HasCollection)),
                FormatSeriesSummary(results));

            logger.LogInformation(
                "IGDB targeted collection-coverage analysis completed.");
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation(
                "IGDB targeted collection-coverage analysis was cancelled.");
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "IGDB targeted collection-coverage analysis failed.");
        }
        finally
        {
            applicationLifetime.StopApplication();
        }
    }

    private static CoverageResult Evaluate(
        ExpectedCollectionCase expected,
        IReadOnlyDictionary<long, IgdbGameSample> gamesById)
    {
        if (!gamesById.TryGetValue(expected.GameId, out var game))
        {
            return new CoverageResult(expected, null, CoverageStatus.MissingRecord);
        }

        if (game.Collections.Count == 0)
        {
            return new CoverageResult(expected, game, CoverageStatus.MissingCollection);
        }

        var expectedCollectionPresent = game.Collections
            .Any(collection => collection.Id == expected.CollectionId);

        return new CoverageResult(
            expected,
            game,
            expectedCollectionPresent
                ? CoverageStatus.Match
                : CoverageStatus.UnexpectedCollection);
    }

    private static string FormatResults(IReadOnlyList<CoverageResult> results) =>
        string.Join(Environment.NewLine, results.Select(result =>
            $"- series={result.Expected.Series}; " +
            $"ID={result.Expected.GameId}; " +
            $"expected title={result.Expected.GameName}; " +
            $"returned title={result.Game?.Name ?? "not returned"}; " +
            $"expected collection={result.Expected.CollectionId}|{result.Expected.CollectionName}; " +
            $"returned collections={FormatCollections(result.Game?.Collections)}; " +
            $"status={FormatStatus(result.Status)}"));

    private static string FormatSeriesSummary(IReadOnlyList<CoverageResult> results) =>
        string.Join(Environment.NewLine, results
            .GroupBy(result => result.Expected.Series)
            .Select(group =>
            {
                var correct = group.Count(result => result.IsExpectedCollectionPresent);
                return $"- {group.Key}: {correct}/{group.Count()} " +
                       $"({Percentage(correct, group.Count()):F2}%) expected collections found";
            }));

    private static string FormatCollections(
        IReadOnlyList<IgdbNamedReference>? collections) =>
        collections is null || collections.Count == 0
            ? "not reported"
            : string.Join(", ", collections
                .OrderBy(collection => collection.Id)
                .Select(collection =>
                    $"{collection.Id}|{FormatValue(collection.Name)}"));

    private static string FormatStatus(CoverageStatus status) => status switch
    {
        CoverageStatus.Match => "MATCH",
        CoverageStatus.MissingRecord => "MISSING RECORD",
        CoverageStatus.MissingCollection => "MISSING COLLECTION",
        CoverageStatus.UnexpectedCollection => "UNEXPECTED COLLECTION",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    private static double Percentage(int numerator, int denominator) =>
        denominator == 0 ? 0 : numerator * 100.0 / denominator;

    private static string FormatValue(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "not reported" : value;

    private sealed record ExpectedCollectionCase(
        string Series,
        long GameId,
        string GameName,
        long CollectionId,
        string CollectionName);

    private sealed record CoverageResult(
        ExpectedCollectionCase Expected,
        IgdbGameSample? Game,
        CoverageStatus Status)
    {
        public bool HasCollection => Game?.Collections.Count > 0;

        public bool IsExpectedCollectionPresent => Status == CoverageStatus.Match;
    }

    private enum CoverageStatus
    {
        Match,
        MissingRecord,
        MissingCollection,
        UnexpectedCollection
    }
}
