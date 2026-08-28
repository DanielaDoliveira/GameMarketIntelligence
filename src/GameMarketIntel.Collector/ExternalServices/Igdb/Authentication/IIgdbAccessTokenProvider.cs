namespace GameMarketIntel.Collector.ExternalServices.Igdb.Authentication;

public interface IIgdbAccessTokenProvider
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken);
    Task<string> RenewAccessTokenAsync(CancellationToken cancellationToken);
}