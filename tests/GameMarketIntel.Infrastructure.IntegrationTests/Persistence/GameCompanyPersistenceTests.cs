using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence;

[Collection(PostgreSqlCollection.Name)]
public sealed class GameCompanyPersistenceTests
{
    private readonly PostgreSqlFixture _fixture;

    public GameCompanyPersistenceTests(PostgreSqlFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task SaveAndLoad_ShouldPersistCompanyParticipationWithProvenance()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var dataSource = CreateDataSource();

        var game = new Game(
            name: "The Legend of Zelda: Ocarina of Time 3D",
            productType: GameProductType.Remake);

        var company = new Company("Grezzo");
        var observedAt = DateTimeOffset.UtcNow;

        var externalGameRecord = new ExternalGameRecord(
            dataSource.Id,
            "game-1",
            observedAt);

        var externalCompanyRecord = new ExternalCompanyRecord(
            dataSource.Id,
            "company-1",
            observedAt);

        externalGameRecord.LinkToGame(game.Id);
        externalCompanyRecord.LinkToCompany(company.Id);

        var gameCompany = new GameCompany(
            externalGameRecord,
            externalCompanyRecord,
            GameCompanyRole.Developer);

        // Act
        await using (var writeDbContext = _fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(dataSource);
            writeDbContext.Games.Add(game);
            writeDbContext.Companies.Add(company);
            writeDbContext.ExternalGameRecords.Add(externalGameRecord);
            writeDbContext.ExternalCompanyRecords.Add(externalCompanyRecord);
            writeDbContext.GameCompanies.Add(gameCompany);

            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = _fixture.CreateDbContext();

        var persistedCompany = await readDbContext.Companies.SingleAsync();
        var persistedExternalCompanyRecord = await readDbContext.ExternalCompanyRecords.SingleAsync();
        var persistedGameCompany = await readDbContext.GameCompanies.SingleAsync();

        // Assert
        persistedCompany.Id.ShouldBe(company.Id);
        persistedCompany.Name.ShouldBe("Grezzo");
        persistedCompany.NormalizedName.ShouldBe("GREZZO");

        persistedExternalCompanyRecord.DataSourceId.ShouldBe(dataSource.Id);
        persistedExternalCompanyRecord.ExternalId.ShouldBe("company-1");
        persistedExternalCompanyRecord.CompanyId.ShouldBe(company.Id);

        persistedGameCompany.GameId.ShouldBe(game.Id);
        persistedGameCompany.CompanyId.ShouldBe(company.Id);
        persistedGameCompany.ExternalGameRecordId.ShouldBe(externalGameRecord.Id);
        persistedGameCompany.ExternalCompanyRecordId.ShouldBe(externalCompanyRecord.Id);
        persistedGameCompany.Role.ShouldBe(GameCompanyRole.Developer);
    }

    private static DataSource CreateDataSource()
    {
        var reliability = new SourceReliability(
            ReliabilityLevel.PublicDirect,
            "Dados obtidos diretamente de uma fonte pública.");

        return new DataSource(
            code: "igdb",
            name: "IGDB",
            url: "https://www.igdb.com",
            reliability,
            attributionRequired: true);
    }
}