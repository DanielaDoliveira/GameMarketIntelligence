namespace GameMarketIntel.Collector.ExternalServices.Igdb;

public interface IIgdbRequestPacer
{
    Task WaitAsync(CancellationToken cancellationToken);
}