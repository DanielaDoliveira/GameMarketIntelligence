namespace GameMarketIntel.Collector.Igdb.Client;

public interface IIgdbRetryDelay
{
    Task WaitAsync(
        TimeSpan delay,
        CancellationToken cancellationToken);
}
