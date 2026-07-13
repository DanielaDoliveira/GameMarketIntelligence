using GameMarketIntel.Application.Abstractions.Persistence;
using GameMarketIntel.Application.Services;
using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;
using Shouldly;

namespace GameMarketIntel.Application.Tests.Services;

public sealed class DataSourceServiceTests
{
    [Fact]
    public async Task GetAllAsync_ShouldMapDataSourcesToDetails()
    {
        // Arrange
        var reliability = new SourceReliability(ReliabilityLevel.Official, "Dados fornecidos pela fonte oficial.", "Pode haver limites de requisições.");

        var dataSource = new DataSource(
                "Steam Web API",
                "https://partner.steamgames.com",
                reliability,
                attributionRequired: true,
                licenseNotes: "Consultar os termos de uso.");

        var repository = new FakeDataSourceRepository(
            [dataSource]);

        var service = new DataSourceService(repository);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Count.ShouldBe(1);

        var source = result[0];

        source.Id.ShouldBe(dataSource.Id);
        source.Name.ShouldBe("Steam Web API");
        source.Url.ShouldBe(dataSource.Url);
        source.AttributionRequired.ShouldBeTrue();
        source.LicenseNotes.ShouldBe(
            "Consultar os termos de uso.");

        source.Reliability.Level.ShouldBe("Official");
        source.Reliability.Reason.ShouldBe(
            "Dados fornecidos pela fonte oficial.");
        source.Reliability.Limitations.ShouldBe(
            "Pode haver limites de requisições.");
    }

    private sealed class FakeDataSourceRepository(
        IReadOnlyList<DataSource> dataSources)
        : IDataSourceRepository
    {
        public Task<IReadOnlyList<DataSource>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(dataSources);
        }
    }
}