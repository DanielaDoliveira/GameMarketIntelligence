using GameMarketIntel.Application.Abstractions.Persistence;
using GameMarketIntel.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;


namespace GameMarketIntel.Infrastructure;

public static class DepedencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IDataSourceRepository, DataSourceRepository>();
        return services;
    }
}
