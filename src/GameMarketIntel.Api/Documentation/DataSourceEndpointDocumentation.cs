using GameMarketIntel.Shared.Contracts.Sources;

namespace GameMarketIntel.Api.Documentation;

public static class DataSourceEndpointDocumentation
{
    public static RouteHandlerBuilder WithGetAllDataSourcesDocumentation(
        this RouteHandlerBuilder builder)
    {
        return builder
            .WithName("GetDataSources")
            .WithSummary("Obtém todas as fontes de dados")
            .WithDescription(
                "Retorna as fontes públicas utilizadas pelo sistema e suas informações de confiabilidade.")
            .Produces<IReadOnlyList<DataSourceDetails>>(
                StatusCodes.Status200OK);
    }
}