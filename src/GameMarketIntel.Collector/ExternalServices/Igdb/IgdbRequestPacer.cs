using Microsoft.Extensions.Options;

namespace GameMarketIntel.Collector.ExternalServices.Igdb;

public class IgdbRequestPacer(IOptions<IgdbOptions> options, TimeProvider timeProvider) : IIgdbRequestPacer
{
    private readonly SemaphoreSlim _accessLock = new(1, 1);
    private readonly TimeSpan _requestInterval = options.Value.RequestInterval;
    private long? _lastRequestTimestamp;

    public async Task WaitAsync(CancellationToken cancellationToken)
    {
        await _accessLock.WaitAsync(cancellationToken);

        try
        {
            var delay = GetRemainingDelay();

            while (delay > TimeSpan.Zero)
            {
                // Round up so a sub-millisecond remainder does not become a zero-delay loop.
                var timerDelay = TimeSpan.FromMilliseconds(Math.Ceiling(delay.TotalMilliseconds));
                await Task.Delay(timerDelay, timeProvider, cancellationToken);
                delay = GetRemainingDelay();
            }

            cancellationToken.ThrowIfCancellationRequested();
            _lastRequestTimestamp = timeProvider.GetTimestamp();
        }
        finally
        {
            _accessLock.Release();
        }
    }

    private TimeSpan GetRemainingDelay() =>
        _lastRequestTimestamp.HasValue
            ? _requestInterval - timeProvider.GetElapsedTime(_lastRequestTimestamp.Value)
            : TimeSpan.Zero;
}