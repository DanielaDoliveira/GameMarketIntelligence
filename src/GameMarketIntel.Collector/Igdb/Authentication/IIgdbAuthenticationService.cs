namespace GameMarketIntel.Collector.Igdb.Authentication;

public interface IIgdbAuthenticationService
{
    Task<IgdbAccessTokenResponse> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    
}