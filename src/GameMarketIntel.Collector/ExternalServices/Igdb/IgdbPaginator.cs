using System.Runtime.CompilerServices;
using System.Text.Json;

namespace GameMarketIntel.Collector.ExternalServices.Igdb;

/// <summary>
/// Reads non-empty JSON pages on demand, without retaining previous pages.
/// Offset pagination does not provide a snapshot of a changing source.
/// </summary>
public sealed class IgdbPaginator(IIgdbClient client)
{
    public async IAsyncEnumerable<string> ReadPagesAsync(
        string endpoint,
        IgdbPageQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Async iterator validation runs when enumeration starts.
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);
        ArgumentNullException.ThrowIfNull(query);

        var offset = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var json = await client.QueryAsync(
                endpoint,
                query.Build(offset),
                cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            var recordCount = CountRecords(json, query.PageSize);

            if (recordCount == 0)
                yield break;

            // The JSON document used for counting is already disposed here.
            yield return json;
            cancellationToken.ThrowIfCancellationRequested();

            if (recordCount < query.PageSize)
                yield break;

            offset = checked(offset + query.PageSize);
        }
    }

    private static int CountRecords(string json, int pageSize)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidOperationException("IGDB returned an empty page body.");
        try
        {
            using var document = JsonDocument.Parse(json);

            if (document.RootElement.ValueKind != JsonValueKind.Array)
                throw new InvalidOperationException("IGDB page must be a JSON array.");

            var count = document.RootElement.GetArrayLength();

            if (count > pageSize)
                throw new InvalidOperationException("IGDB page exceeded the requested page size.");

            return count;
        }
        catch (JsonException)
        {
            // Do not expose the response body or parser diagnostics.
            throw new InvalidOperationException("IGDB returned invalid page JSON.");
        }
    }
}