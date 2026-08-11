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
    private static readonly IReadOnlyList<ReferenceCase> ReferenceCases =
    [
        new("The Legend of Zelda", 1029, "The Legend of Zelda: Ocarina of Time"),
        new("The Legend of Zelda", 1036, "The Legend of Zelda: Twilight Princess"),
        new("The Legend of Zelda", 119388, "The Legend of Zelda: Tears of the Kingdom"),

        new("Mario", 26758, "Super Mario Odyssey"),
        new("Mario", 2350, "Mario Kart 8"),
        new("Mario", 1077, "Super Mario Galaxy"),

        new("Pokémon", 1561, "Pokémon Red Version"),
        new("Pokémon", 37382, "Pokémon Sword"),
        new("Pokémon", 144054, "Pokémon Legends: Arceus"),

        new("Kingdom Hearts", 393742, "Kingdom Hearts"),
        new("Kingdom Hearts", 1221, "Kingdom Hearts II"),
        new("Kingdom Hearts", 2933, "Kingdom Hearts III"),

        new("Final Fantasy", 393025, "Final Fantasy VII"),
        new("Final Fantasy", 418, "Final Fantasy X"),
        new("Final Fantasy", 31551, "Final Fantasy XVI"),

        new("Hollow Knight", 14593, "Hollow Knight"),
        new("Hollow Knight", 115289, "Hollow Knight: Silksong"),

        new("Animal Crossing", 2655, "Animal Crossing"),
        new("Animal Crossing", 2687, "Animal Crossing: New Leaf"),
        new("Animal Crossing", 109462, "Animal Crossing: New Horizons"),

        new("Splatoon", 7335, "Splatoon"),
        new("Splatoon", 26761, "Splatoon 2"),
        new("Splatoon", 143613, "Splatoon 3"),

        new("Grand Theft Auto", 730, "Grand Theft Auto III"),
        new("Grand Theft Auto", 732, "Grand Theft Auto: San Andreas"),
        new("Grand Theft Auto", 1020, "Grand Theft Auto V")
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation(
                """
                Starting IGDB targeted franchise-versus-collection analysis:
                Reference cases: {CaseCount}
                Reference series: {SeriesCount}
                """,
                ReferenceCases.Count,
                ReferenceCases.Select(item => item.Series).Distinct().Count());

            var tokenResponse =
                await authenticationService.GetAccessTokenAsync(stoppingToken);

            if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                throw new InvalidOperationException(
                    "Twitch returned an empty access token.");
            }

            var games = await igdbClient.GetGamesByIdsAsync(
                tokenResponse.AccessToken,
                ReferenceCases.Select(item => item.GameId).ToArray(),
                stoppingToken);

            var gamesById = games.ToDictionary(game => game.Id);
            var results = ReferenceCases
                .Select(reference => Evaluate(reference, gamesById))
                .ToArray();

            logger.LogInformation(
                """
                IGDB targeted franchise-versus-collection results:
                {Results}
                """,
                FormatResults(results));

            logger.LogInformation(
                """
                IGDB targeted franchise-versus-collection summary:
                Reference cases: {ReferenceCases}
                Records returned: {ReturnedRecords}
                Records with one or more franchises: {WithFranchises}
                Records with one or more collections: {WithCollections}
                Records with both fields: {WithBoth}
                Records with neither field: {WithNeither}
                Missing records: {MissingRecords}
                Franchise presence coverage: {FranchiseCoverage:F2}%
                Collection presence coverage: {CollectionCoverage:F2}%
                Records where at least one franchise label also appears as a collection label: {WithLabelOverlap}
                Records where at least one franchise label is distinct from every collection label: {WithDistinctFranchiseLabel}

                Franchise presence by reference series:
                {SeriesSummary}
                """,
                results.Length,
                results.Count(result => result.Game is not null),
                results.Count(result => result.HasFranchise),
                results.Count(result => result.HasCollection),
                results.Count(result => result.HasFranchise && result.HasCollection),
                results.Count(result => result.Game is not null && !result.HasFranchise && !result.HasCollection),
                results.Count(result => result.Game is null),
                Percentage(results.Count(result => result.HasFranchise), results.Length),
                Percentage(results.Count(result => result.HasCollection), results.Length),
                results.Count(result => result.HasMatchingLabel),
                results.Count(result => result.HasDistinctFranchiseLabel),
                FormatSeriesSummary(results));

            logger.LogInformation(
                "IGDB targeted franchise-versus-collection analysis completed.");
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation(
                "IGDB targeted franchise-versus-collection analysis was cancelled.");
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "IGDB targeted franchise-versus-collection analysis failed.");
        }
        finally
        {
            applicationLifetime.StopApplication();
        }
    }

    private static ComparisonResult Evaluate(
        ReferenceCase reference,
        IReadOnlyDictionary<long, IgdbGameSample> gamesById)
    {
        if (!gamesById.TryGetValue(reference.GameId, out var game))
        {
            return new ComparisonResult(reference, null, false, false);
        }

        var collectionNames = game.Collections
            .Select(collection => collection.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var hasMatchingLabel = game.Franchises
            .Any(franchise => collectionNames.Contains(franchise.Name));

        var hasDistinctFranchiseLabel = game.Franchises
            .Any(franchise => !string.IsNullOrWhiteSpace(franchise.Name) &&
                              !collectionNames.Contains(franchise.Name));

        return new ComparisonResult(
            reference,
            game,
            hasMatchingLabel,
            hasDistinctFranchiseLabel);
    }

    private static string FormatResults(IReadOnlyList<ComparisonResult> results) =>
        string.Join(Environment.NewLine, results.Select(result =>
            $"- reference series={result.Reference.Series}; " +
            $"ID={result.Reference.GameId}; " +
            $"expected title={result.Reference.GameName}; " +
            $"returned title={result.Game?.Name ?? "not returned"}; " +
            $"franchises={FormatReferences(result.Game?.Franchises)}; " +
            $"collections={FormatReferences(result.Game?.Collections)}; " +
            $"same-label overlap={FormatBoolean(result.HasMatchingLabel)}; " +
            $"distinct franchise label={FormatBoolean(result.HasDistinctFranchiseLabel)}"));

    private static string FormatSeriesSummary(IReadOnlyList<ComparisonResult> results) =>
        string.Join(Environment.NewLine, results
            .GroupBy(result => result.Reference.Series)
            .Select(group =>
            {
                var withFranchise = group.Count(result => result.HasFranchise);
                return $"- {group.Key}: {withFranchise}/{group.Count()} " +
                       $"({Percentage(withFranchise, group.Count()):F2}%) with franchises";
            }));

    private static string FormatReferences(
        IReadOnlyList<IgdbNamedReference>? references) =>
        references is null || references.Count == 0
            ? "not reported"
            : string.Join(", ", references
                .OrderBy(reference => reference.Id)
                .Select(reference =>
                    $"{reference.Id}|{FormatValue(reference.Name)}"));

    private static string FormatBoolean(bool value) => value ? "YES" : "NO";

    private static double Percentage(int numerator, int denominator) =>
        denominator == 0 ? 0 : numerator * 100.0 / denominator;

    private static string FormatValue(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "not reported" : value;

    private sealed record ReferenceCase(
        string Series,
        long GameId,
        string GameName);

    private sealed record ComparisonResult(
        ReferenceCase Reference,
        IgdbGameSample? Game,
        bool HasMatchingLabel,
        bool HasDistinctFranchiseLabel)
    {
        public bool HasFranchise => Game?.Franchises.Count > 0;

        public bool HasCollection => Game?.Collections.Count > 0;
    }
}
