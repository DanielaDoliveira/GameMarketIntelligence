using GameMarketIntel.Application.Abstractions.Services;
using GameMarketIntel.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GameMarketIntel.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IDataSourceService, DataSourceService>();

        return services;
    }
}