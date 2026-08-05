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
    
    Task<IReadOnlyList<IgdbGameTypeReference>> GetGameTypesAsync(
        string accessToken,
        CancellationToken cancellationToken = default
        );
    Task<IReadOnlyList<IgdbGameSample>> GetGamesIncludedInBundleAsync(
        string accessToken,
        long bundleId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IgdbGameSample>> SearchGamesByNameAsync(
        string accessToken,
        string gameName,
        int resultLimit,
        CancellationToken cancellationToken = default);
    
    
    Task<IReadOnlyList<IgdbGameSample>> GetGamesAlternativeNamesSampleAsync(
        string accessToken,
        IReadOnlyCollection<long> gameIds,
        CancellationToken cancellationToken = default);
    Task<int> CountReleasedGamesAsync(
        string accessToken,
        long releaseDateCutoff,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IgdbGameSample>> GetReleasedGameAtOffsetAsync(
        string accessToken,
        long releaseDateCutoff,
        int offset,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IgdbGameSample>> GetReleasedGamesAtOffsetsAsync(
        string accessToken,
        long releaseDateCutoff,
        IReadOnlyCollection<int> offsets,
        CancellationToken cancellationToken = default);
}