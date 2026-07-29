using GameMarketIntel.Collector.Igdb.Contracts;

namespace GameMarketIntel.Collector.Igdb.Client;

public interface IIgdbClient
{
    Task<IReadOnlyList<IgdbGameSample>> GetGamesSampleAsync(string accessToken, int sampleSize, CancellationToken cancellationToken = default);
}