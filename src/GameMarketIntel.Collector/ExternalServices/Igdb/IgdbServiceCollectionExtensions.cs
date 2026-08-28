using System.Net.Http;
using GameMarketIntel.Collector.ExternalServices.Igdb.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace GameMarketIntel.Collector.ExternalServices.Igdb;

public static class IgdbServiceCollectionExtensions
{
    public static IServiceCollection AddIgdbIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<IgdbOptions>()
            .Bind(configuration.GetSection(IgdbOptions.SectionName))
            .ValidateOnStart();

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<IgdbOptions>, IgdbOptionsValidator>());
        services.TryAddSingleton(TimeProvider.System);

        ConfigureHttpTransport(services.AddHttpClient(TwitchOAuthClient.HttpClientName));
        ConfigureHttpTransport(services.AddHttpClient(IgdbClient.HttpClientName, ConfigureIgdbHttpClient));

        services.TryAddSingleton<ITwitchOAuthClient, TwitchOAuthClient>();
        services.TryAddSingleton<IIgdbAccessTokenProvider, IgdbAccessTokenProvider>();
        services.TryAddSingleton<IIgdbRequestPacer, IgdbRequestPacer>();
        services.TryAddSingleton<IIgdbRetryPolicy, IgdbRetryPolicy>();
        services.TryAddSingleton<IgdbQueryTimeout>();
        services.TryAddSingleton<IIgdbClient, IgdbClient>();
        services.TryAddSingleton<IgdbPaginator>();
        return services;
    }

    private static void ConfigureIgdbHttpClient(IServiceProvider serviceProvider, HttpClient client)
    {
        var options = serviceProvider.GetRequiredService<IOptions<IgdbOptions>>().Value;
        client.BaseAddress = options.ApiBaseAddress;
        client.DefaultRequestHeaders.Add("Client-ID", options.ClientId);
    }

    private static void ConfigureHttpTransport(IHttpClientBuilder builder) =>
        builder
            // IgdbQueryTimeout owns the complete query deadline, including OAuth.
            .ConfigureHttpClient(client => client.Timeout = Timeout.InfiniteTimeSpan)
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                AllowAutoRedirect = false,
                UseCookies = false
            })
            .RedactLoggedHeaders(_ => true);
}
