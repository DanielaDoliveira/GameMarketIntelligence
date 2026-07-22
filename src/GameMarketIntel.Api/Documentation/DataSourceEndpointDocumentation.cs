
using GameMarketIntel.Shared.Contracts.Sources;

namespace GameMarketIntel.Api.Documentation;

public static class DataSourceEndpointDocumentation
{
    public static RouteHandlerBuilder WithGetAllDataSourcesDocumentation(
        this RouteHandlerBuilder builder)
    {
        return builder
            .WithName("GetDataSources")
            .WithSummary("Gets all data sources")
            .WithDescription(
                "Returns the public data sources used by the system and their reliability information.")
            .Produces<IReadOnlyList<DataSourceDetails>>(
                StatusCodes.Status200OK);
    }
}
