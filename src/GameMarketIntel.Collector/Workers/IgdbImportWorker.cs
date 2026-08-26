using GameMarketIntel.Collector.Jobs;

namespace GameMarketIntel.Collector.Workers;

public sealed class IgdbImportWorker(
    IIgdbImportJob importJob,
    IHostApplicationLifetime applicationLifetime,
    ILogger<IgdbImportWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("===IGDB import started===");
        try
        {
            await importJob.ExecuteAsync(stoppingToken);
            logger.LogInformation("===IGDB import completed===");
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("IGDB import was cancelled");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "IGDB import failed");
            throw;
        }
        finally
        {
            applicationLifetime.StopApplication();
        }
    }
}