using GameMarketIntel.Application.Abstractions.Services;
using GameMarketIntel.Application.Games.Search;
using GameMarketIntel.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;


namespace GameMarketIntel.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<SearchGamesQueryValidator>();
        services.AddScoped<IDataSourceService, DataSourceService>();
        services.AddScoped<IGameSearchService, GameSearchService>();

        return services;
    }
}