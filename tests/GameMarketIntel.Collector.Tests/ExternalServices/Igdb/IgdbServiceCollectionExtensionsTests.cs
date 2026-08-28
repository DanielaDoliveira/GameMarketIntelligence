
using GameMarketIntel.Collector.ExternalServices;
using GameMarketIntel.Collector.ExternalServices.Igdb;
using GameMarketIntel.Collector.ExternalServices.Igdb.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Http;
using Shouldly;

namespace GameMarketIntel.Collector.Tests.ExternalServices.Igdb;

public sealed class IgdbServiceCollectionExtensionsTests
{
    [Fact]
    public void AddIgdbIntegration_ShouldBindAndValidateOptions()
    {
        // Arrange
        const string clientId = "client-id";
        const string clientSecret = "client-secret";
        var configuration = CreateConfiguration(clientId, clientSecret);
        var services = new ServiceCollection();

        // Act
        services.AddIgdbIntegration(configuration);
        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<IgdbOptions>>().Value;
        var validators = serviceProvider.GetServices<IValidateOptions<IgdbOptions>>();

        // Assert
        options.ClientId.ShouldBe(clientId);
        options.ClientSecret.ShouldBe(clientSecret);
        validators.ShouldContain(validator => validator is IgdbOptionsValidator);
    }

    [Fact]
    public void AddIgdbIntegration_ShouldRegisterAuthenticationPacingAndRetryServices()
    {
        // Arrange
        var configuration = CreateConfiguration("client-id", "client-secret");
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddIgdbIntegration(configuration);
        using var serviceProvider = services.BuildServiceProvider();
        var firstOAuthClient = serviceProvider.GetRequiredService<ITwitchOAuthClient>();
        var secondOAuthClient = serviceProvider.GetRequiredService<ITwitchOAuthClient>();
        var firstTokenProvider = serviceProvider.GetRequiredService<IIgdbAccessTokenProvider>();
        var secondTokenProvider = serviceProvider.GetRequiredService<IIgdbAccessTokenProvider>();
        var firstRequestPacer = serviceProvider.GetRequiredService<IIgdbRequestPacer>();
        var secondRequestPacer = serviceProvider.GetRequiredService<IIgdbRequestPacer>();
        var firstRetryPolicy = serviceProvider.GetRequiredService<IIgdbRetryPolicy>();
        var secondRetryPolicy = serviceProvider.GetRequiredService<IIgdbRetryPolicy>();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

        // Assert
        firstOAuthClient.ShouldBeOfType<TwitchOAuthClient>();
        secondOAuthClient.ShouldBeSameAs(firstOAuthClient);
        firstTokenProvider.ShouldBeOfType<IgdbAccessTokenProvider>();
        secondTokenProvider.ShouldBeSameAs(firstTokenProvider);
        firstRequestPacer.ShouldBeOfType<IgdbRequestPacer>();
        secondRequestPacer.ShouldBeSameAs(firstRequestPacer);
        firstRetryPolicy.ShouldBeOfType<IgdbRetryPolicy>();
        secondRetryPolicy.ShouldBeSameAs(firstRetryPolicy);
        httpClientFactory.ShouldNotBeNull();
        serviceProvider.GetRequiredService<TimeProvider>().ShouldBeSameAs(TimeProvider.System);
    }

    [Fact]
    public void AddIgdbIntegration_ShouldRegisterQueryTimeoutAsSingleton()
    {
        // Arrange
        var configuration = CreateConfiguration("client-id", "client-secret");
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddIgdbIntegration(configuration);
        using var serviceProvider = services.BuildServiceProvider();
        var firstTimeout = serviceProvider.GetRequiredService<IgdbQueryTimeout>();
        var secondTimeout = serviceProvider.GetRequiredService<IgdbQueryTimeout>();

        // Assert
        firstTimeout.ShouldNotBeNull();
        secondTimeout.ShouldBeSameAs(firstTimeout);
    }

    [Fact]
    public void AddIgdbIntegration_ShouldRegisterIgdbClientAsSingleton()
    {
        // Arrange
        var configuration = CreateConfiguration("client-id", "client-secret");
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddIgdbIntegration(configuration);
        using var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        var firstClient = serviceProvider.GetRequiredService<IIgdbClient>();
        var secondClient = serviceProvider.GetRequiredService<IIgdbClient>();

        // Assert
        firstClient.ShouldBeOfType<IgdbClient>();
        secondClient.ShouldBeSameAs(firstClient);
    }

    [Fact]
    public void AddIgdbIntegration_ShouldConfigureIgdbHttpClientFromOptions()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Igdb:ClientId"] = "configured-client-id",
                ["Igdb:ClientSecret"] = "synthetic-client-secret",
                ["Igdb:ApiBaseAddress"] = "https://example.invalid/custom/v4/"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddIgdbIntegration(configuration);
        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        using var client = factory.CreateClient(IgdbClient.HttpClientName);

        // Assert
        client.BaseAddress.ShouldBe(new Uri("https://example.invalid/custom/v4/"));
        client.DefaultRequestHeaders.GetValues("Client-ID").Single().ShouldBe("configured-client-id");
        client.DefaultRequestHeaders.Authorization.ShouldBeNull();
        client.DefaultRequestHeaders.Contains("Client-Secret").ShouldBeFalse();
    }

    [Theory]
    [InlineData(IgdbClient.HttpClientName)]
    [InlineData(TwitchOAuthClient.HttpClientName)]
    public void AddIgdbIntegration_ShouldDelegateHttpTimeoutToQueryDeadline(string clientName)
    {
        // Arrange
        var configuration = CreateConfiguration("client-id", "client-secret");
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddIgdbIntegration(configuration);
        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        using var client = factory.CreateClient(clientName);

        // Assert
        client.Timeout.ShouldBe(Timeout.InfiniteTimeSpan);
    }

    [Theory]
    [InlineData(IgdbClient.HttpClientName)]
    [InlineData(TwitchOAuthClient.HttpClientName)]
    public void AddIgdbIntegration_ShouldDisableAutomaticRedirectsAndCookies(string clientName)
    {
        // Arrange
        var configuration = CreateConfiguration("client-id", "client-secret");
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddIgdbIntegration(configuration);
        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IHttpMessageHandlerFactory>();
        var handler = factory.CreateHandler(clientName);

        // Follow the factory wrappers to inspect our configured transport.
        while (handler is DelegatingHandler wrapper)
            handler = wrapper.InnerHandler!;

        // Assert
        var transport = handler.ShouldBeOfType<SocketsHttpHandler>();
        transport.AllowAutoRedirect.ShouldBeFalse();
        transport.UseCookies.ShouldBeFalse();
    }

    [Theory]
    [InlineData(IgdbClient.HttpClientName)]
    [InlineData(TwitchOAuthClient.HttpClientName)]
    public void AddIgdbIntegration_ShouldRedactHttpHeaderValues(string clientName)
    {
        // Arrange
        var configuration = CreateConfiguration("client-id", "client-secret");
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddIgdbIntegration(configuration);
        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptionsMonitor<HttpClientFactoryOptions>>().Get(clientName);

        // Assert
        options.ShouldRedactHeaderValue("Authorization").ShouldBeTrue();
        options.ShouldRedactHeaderValue("Client-ID").ShouldBeTrue();
        options.ShouldRedactHeaderValue("X-Diagnostic").ShouldBeTrue();
    }

    [Fact]
    public void AddIgdbIntegration_ShouldRegisterPaginatorAsSingleton()
    {
        // Arrange
        var configuration = CreateConfiguration("client-id", "client-secret");
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddIgdbIntegration(configuration);
        using var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        var firstPaginator = serviceProvider.GetRequiredService<IgdbPaginator>();
        var secondPaginator = serviceProvider.GetRequiredService<IgdbPaginator>();

        // Assert
        firstPaginator.ShouldNotBeNull();
        secondPaginator.ShouldBeSameAs(firstPaginator);
    }

    [Fact]
    public void AddIgdbIntegration_ShouldNotDuplicatePaginatorRegistration()
    {
        // Arrange
        var configuration = CreateConfiguration("client-id", "client-secret");
        var services = new ServiceCollection();

        // Act
        services.AddIgdbIntegration(configuration);
        services.AddIgdbIntegration(configuration);

        // Assert
        var registration = services.Single(service => service.ServiceType == typeof(IgdbPaginator));
        registration.ImplementationType.ShouldBe(typeof(IgdbPaginator));
        registration.Lifetime.ShouldBe(ServiceLifetime.Singleton);
    }

    private static IConfiguration CreateConfiguration(string clientId, string clientSecret) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{IgdbOptions.SectionName}:ClientId"] = clientId,
                [$"{IgdbOptions.SectionName}:ClientSecret"] = clientSecret
            })
            .Build();
}
