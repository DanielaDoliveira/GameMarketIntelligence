using GameMarketIntel.Collector.Extensions;
using GameMarketIntel.Collector.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;

namespace GameMarketIntel.Collector.Tests.Extensions;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCollector_ShouldRegisterIgdbImportWorkerAsHostedService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCollector();

        // Assert
        var registration = services.Single(descriptor =>
            descriptor.ServiceType == typeof(IHostedService));
        registration.ImplementationType.ShouldBe(typeof(IgdbImportWorker));
        registration.Lifetime.ShouldBe(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddCollector_WhenCalledTwice_ShouldNotDuplicateWorkerRegistration()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCollector();
        services.AddCollector();

        // Assert
        var registrationCount = services.Count(descriptor =>
            descriptor.ServiceType == typeof(IHostedService) &&
            descriptor.ImplementationType == typeof(IgdbImportWorker));
        registrationCount.ShouldBe(1);
    }
}