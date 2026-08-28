namespace GameMarketIntel.Collector.ExternalServices.Igdb;

public interface IIgdbClient
{
    Task<string> QueryAsync(string endpoint, string query, CancellationToken cancellationToken);
}