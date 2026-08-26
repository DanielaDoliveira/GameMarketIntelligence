namespace GameMarketIntel.Collector.Jobs;

public interface IIgdbImportJob
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}