namespace GameMarketIntel.Collector.ExternalServices.Igdb.Authentication;

public sealed class IgdbAccessTokenProvider(
    ITwitchOAuthClient oAuthClient,
    TimeProvider timeProvider)
    : IIgdbAccessTokenProvider
{
    private static readonly TimeSpan ExpirationSafetyWindow = TimeSpan.FromMinutes(1);
    private readonly SemaphoreSlim _accessLock = new(1, 1);
    private TwitchAccessToken? _cachedToken;

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        await _accessLock.WaitAsync(cancellationToken);

        try
        {
            if (HasUsableAccessToken())
                return _cachedToken!.Value;

            return await RequestAndCacheAccessTokenAsync(cancellationToken);
        }
        finally
        {
            _accessLock.Release();
        }
    }

    public async Task<string> RenewAccessTokenAsync(CancellationToken cancellationToken)
    {
        await _accessLock.WaitAsync(cancellationToken);

        try
        {
            return await RequestAndCacheAccessTokenAsync(cancellationToken);
        }
        finally
        {
            _accessLock.Release();
        }
    }

    private bool HasUsableAccessToken() =>
        _cachedToken is not null &&
        timeProvider.GetUtcNow() < _cachedToken.ExpiresAt.Subtract(ExpirationSafetyWindow);

    private async Task<string> RequestAndCacheAccessTokenAsync(CancellationToken cancellationToken)
    {
        _cachedToken = await oAuthClient.RequestAccessTokenAsync(cancellationToken);
        return _cachedToken.Value;
    }
}