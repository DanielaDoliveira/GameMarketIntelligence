namespace GameMarketIntel.Collector.Igdb.Client;

public sealed class IgdbRetryDelay : IIgdbRetryDelay
{
    public Task WaitAsync(
        TimeSpan delay,
        CancellationToken cancellationToken) =>
        Task.Delay(delay, cancellationToken);
}
