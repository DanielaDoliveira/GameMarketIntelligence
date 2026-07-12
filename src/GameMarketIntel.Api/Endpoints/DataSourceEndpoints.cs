using GameMarketIntel.Api.Documentation;
using GameMarketIntel.Application.Abstractions.Services;
using GameMarketIntel.Shared.Contracts.Sources;

namespace GameMarketIntel.Api.Endpoints;

public static class DataSourceEndpoints
{
    public static IEndpointRouteBuilder MapDataSourceEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/data-sources", async
            (
                    IDataSourceService dataSourceService, CancellationToken cancellationToken) =>
                {
                    IReadOnlyList<DataSourceDetails> dataSources =
                       await dataSourceService.GetAllAsync(cancellationToken);

                    return Results.Ok(dataSources);
                })
          .WithGetAllDataSourcesDocumentation();
        return endpoints;
    }
}