using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence;

[Collection(PostgreSqlCollection.Name)]
public sealed class ExternalPlayerPerspectiveRecordPersistenceTests
{
    private readonly PostgreSqlFixture _fixture;

    public ExternalPlayerPerspectiveRecordPersistenceTests(PostgreSqlFixture fixture)=>_fixture = fixture;
    

    [Fact]
    public async Task SaveAndLoad_ShouldPersistExternalPlayerPerspectiveRecord()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var dataSource = CreateDataSource();

        var observedAt = new DateTimeOffset(2026, 8, 20, 10, 0, 0, TimeSpan.Zero);

        var record = new ExternalPlayerPerspectiveRecord(dataSource.Id, "1", observedAt);

        // Act
        await using (var writeDbContext = _fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(dataSource);
            writeDbContext.ExternalPlayerPerspectiveRecords.Add(record);
            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = _fixture.CreateDbContext();
        var persistedRecord = await readDbContext
            .ExternalPlayerPerspectiveRecords
            .SingleAsync();

        // Assert
        persistedRecord.Id.ShouldBe(record.Id);
        persistedRecord.DataSourceId.ShouldBe(dataSource.Id);
        persistedRecord.ExternalId.ShouldBe("1");
        persistedRecord.PlayerPerspectiveId.ShouldBeNull();
        persistedRecord.FirstSeenAt.ShouldBe(observedAt);
        persistedRecord.LastSeenAt.ShouldBe(observedAt);
        persistedRecord.SourceUpdatedAt.ShouldBeNull();
    }

    [Fact]
    public async Task SaveChanges_ShouldRejectDuplicateSourceAndExternalId()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var dataSource = CreateDataSource();
        var observedAt = DateTimeOffset.UtcNow;

        var firstRecord = new ExternalPlayerPerspectiveRecord(dataSource.Id, "1", observedAt);

        var duplicateRecord = new ExternalPlayerPerspectiveRecord(dataSource.Id, "1", observedAt);

        await using var dbContext = _fixture.CreateDbContext();
        dbContext.DataSources.Add(dataSource);
        dbContext.ExternalPlayerPerspectiveRecords.AddRange(firstRecord, duplicateRecord);

        // Act
        var action = () => dbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveAndLoad_ShouldPersistOptionalPlayerPerspectiveLink()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var dataSource = CreateDataSource();
        var playerPerspective = new PlayerPerspective("First person");

        var record = new ExternalPlayerPerspectiveRecord(dataSource.Id, "1", DateTimeOffset.UtcNow);

        record.LinkToPlayerPerspective(playerPerspective.Id);

        // Act
        await using (var writeDbContext = _fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(dataSource);
            writeDbContext.PlayerPerspectives.Add(playerPerspective);
            writeDbContext.ExternalPlayerPerspectiveRecords.Add(record);
            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = _fixture.CreateDbContext();
        var persistedRecord = await readDbContext.ExternalPlayerPerspectiveRecords .SingleAsync();

        // Assert
        persistedRecord.PlayerPerspectiveId.ShouldBe(playerPerspective.Id);
    }

    [Fact]
    public async Task DeletePlayerPerspective_ShouldFail_WhenExternalRecordReferencesIt()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var dataSource = CreateDataSource();
        var playerPerspective = new PlayerPerspective("First person");

        var record = new ExternalPlayerPerspectiveRecord(dataSource.Id, "1", DateTimeOffset.UtcNow);

        record.LinkToPlayerPerspective(playerPerspective.Id);

        await using (var setupDbContext = _fixture.CreateDbContext())
        {
            setupDbContext.DataSources.Add(dataSource);
            setupDbContext.PlayerPerspectives.Add(playerPerspective);
            setupDbContext.ExternalPlayerPerspectiveRecords.Add(record);
            await setupDbContext.SaveChangesAsync();
        }

        await using var deleteDbContext = _fixture.CreateDbContext();
        var persistedPlayerPerspective = await deleteDbContext.PlayerPerspectives.SingleAsync();

        deleteDbContext.PlayerPerspectives.Remove(persistedPlayerPerspective);

        // Act
        var action = () => deleteDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
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