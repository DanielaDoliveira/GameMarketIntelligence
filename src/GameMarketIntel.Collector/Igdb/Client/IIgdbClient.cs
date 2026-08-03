using GameMarketIntel.Collector.Igdb.Contracts;

namespace GameMarketIntel.Collector.Igdb.Client;

public interface IIgdbClient
{
    Task<IReadOnlyList<IgdbGameSample>> GetGamesSampleAsync(
        string accessToken,
        int sampleSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IgdbGameSample>> GetGamesByIdsAsync(
        string accessToken, 
        IReadOnlyCollection<long> gameIds, 
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IgdbGameSample>> GetGamesWithParentAsync(
        string accessToken,
        int sampleSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IgdbGameSample>> GetGamesByTypeAsync(
        string accessToken,
        long gameTypeId,
        int sampleSize,
        CancellationToken cancellationToken = default
    );
}