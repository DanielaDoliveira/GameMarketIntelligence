namespace GameMarketIntel.Collector.ExternalServices.Igdb;

public interface IIgdbRetryPolicy
{
    TimeSpan? GetRetryDelay(HttpResponseMessage response, int retryAttempt);
}