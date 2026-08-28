namespace GameMarketIntel.Collector.ExternalServices.Igdb.Authentication;

public interface ITwitchOAuthClient
{
    Task<TwitchAccessToken> RequestAccessTokenAsync(CancellationToken cancellationToken);
}
