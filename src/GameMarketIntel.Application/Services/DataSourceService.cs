using GameMarketIntel.Application.Abstractions.Persistence;
using GameMarketIntel.Application.Abstractions.Services;
using GameMarketIntel.Shared.Contracts.Sources;

namespace GameMarketIntel.Application.Services;

public sealed class DataSourceService(
    IDataSourceRepository dataSourceRepository)
    : IDataSourceService
{
    public async Task<IReadOnlyList<DataSourceDetails>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var dataSources = await dataSourceRepository.GetAllAsync(
            cancellationToken);

        return dataSources
            .Select(source => new DataSourceDetails(
                source.Id,
                source.Name,
                source.Url,
                source.LicenseNotes,
                source.AttributionRequired,
                new SourceReliabilityDetails(
                    source.Reliability.Level.ToString(),
                    source.Reliability.Reason,
                    source.Reliability.Limitations)))
            .ToList();
    }
}