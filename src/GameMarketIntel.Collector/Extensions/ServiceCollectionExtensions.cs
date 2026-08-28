using GameMarketIntel.Collector.Workers;
namespace GameMarketIntel.Collector.Extensions;
public static class ServiceCollectionExtensions

{
    public static IServiceCollection AddCollector(this IServiceCollection services)
    {
        services.AddHostedService<IgdbImportWorker>();
        return services;
    }
}