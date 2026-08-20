using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence;

[Collection(PostgreSqlCollection.Name)]
public sealed class ExternalGameModeRecordPersistenceTests
{
    private readonly PostgreSqlFixture _fixture;

    public ExternalGameModeRecordPersistenceTests(PostgreSqlFixture fixture) =>   _fixture = fixture;
    

    [Fact]
    public async Task SaveAndLoad_ShouldPersistExternalGameModeRecord()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var dataSource = CreateDataSource();

        var observedAt = new DateTimeOffset(2026, 8, 20, 10, 0, 0, TimeSpan.Zero);

        var record = new ExternalGameModeRecord(dataSource.Id, "1", observedAt);

        // Act
        await using (var writeDbContext = _fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(dataSource);
            writeDbContext.ExternalGameModeRecords.Add(record);
            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = _fixture.CreateDbContext();
        var persistedRecord = await readDbContext.ExternalGameModeRecords.SingleAsync();

        // Assert
        persistedRecord.Id.ShouldBe(record.Id);
        persistedRecord.DataSourceId.ShouldBe(dataSource.Id);
        persistedRecord.ExternalId.ShouldBe("1");
        persistedRecord.GameModeId.ShouldBeNull();
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

        var firstRecord = new ExternalGameModeRecord(dataSource.Id, "1", observedAt);

        var duplicateRecord = new ExternalGameModeRecord(dataSource.Id, "1", observedAt);

        await using var dbContext = _fixture.CreateDbContext();
        dbContext.DataSources.Add(dataSource);
        dbContext.ExternalGameModeRecords.AddRange(firstRecord, duplicateRecord);

        // Act
        var action = () => dbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveAndLoad_ShouldPersistOptionalGameModeLink()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var dataSource = CreateDataSource();
        var gameMode = new GameMode("Single player");

        var record = new ExternalGameModeRecord(dataSource.Id, "1", DateTimeOffset.UtcNow);

        record.LinkToGameMode(gameMode.Id);

        // Act
        await using (var writeDbContext = _fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(dataSource);
            writeDbContext.GameModes.Add(gameMode);
            writeDbContext.ExternalGameModeRecords.Add(record);
            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = _fixture.CreateDbContext();
        var persistedRecord = await readDbContext
            .ExternalGameModeRecords
            .SingleAsync();

        // Assert
        persistedRecord.GameModeId.ShouldBe(gameMode.Id);
    }

    [Fact]
    public async Task DeleteGameMode_ShouldFail_WhenExternalGameModeRecordReferencesIt()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var dataSource = CreateDataSource();
        var gameMode = new GameMode("Single player");

        var record = new ExternalGameModeRecord(dataSource.Id, "1", DateTimeOffset.UtcNow);

        record.LinkToGameMode(gameMode.Id);

        await using (var setupDbContext = _fixture.CreateDbContext())
        {
            setupDbContext.DataSources.Add(dataSource);
            setupDbContext.GameModes.Add(gameMode);
            setupDbContext.ExternalGameModeRecords.Add(record);
            await setupDbContext.SaveChangesAsync();
        }

        await using var deleteDbContext = _fixture.CreateDbContext();
        var persistedGameMode = await deleteDbContext.GameModes .SingleAsync();

        deleteDbContext.GameModes.Remove(persistedGameMode);

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