using Microsoft.Extensions.Options;

namespace GameMarketIntel.Collector.ExternalServices.Igdb;

public sealed class IgdbQueryTimeout(IOptions<IgdbOptions> options, TimeProvider timeProvider)
{
    private readonly TimeSpan _requestTimeout = options.Value.RequestTimeout;

    public async Task<string> ExecuteAsync(Func<CancellationToken, Task<string>> operation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        cancellationToken.ThrowIfCancellationRequested();

        using var timeoutSource = new CancellationTokenSource(_requestTimeout, timeProvider);
        using var linkedSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);

        try
        {
            linkedSource.Token.ThrowIfCancellationRequested();
            var result = await operation(linkedSource.Token);
            linkedSource.Token.ThrowIfCancellationRequested();
            return result;
        }
        catch (OperationCanceledException) when (timeoutSource.IsCancellationRequested &&
                                                 !cancellationToken.IsCancellationRequested)
        {
            // Do not attach an underlying exception that could contain remote details.
            throw new TimeoutException("IGDB query exceeded the configured timeout.");
        }
    }
}