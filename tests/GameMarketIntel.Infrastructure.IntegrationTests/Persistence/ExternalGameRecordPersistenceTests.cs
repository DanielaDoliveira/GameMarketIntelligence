using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence;

[Collection(PostgreSqlCollection.Name)]
public sealed class ExternalGameRecordPersistenceTests
{
    private static readonly DateTimeOffset ObservedAt =
        new(2026, 8, 18, 12, 0, 0, TimeSpan.Zero);

    private readonly PostgreSqlFixture _fixture;

    public ExternalGameRecordPersistenceTests(
        PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SaveAndLoad_ShouldPersistUnlinkedExternalRecord()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var source = CreateDataSource(
            code: "igdb",
            name: "IGDB");

        var record = new ExternalGameRecord(
            source.Id,
            "144542",
            ObservedAt);

        // Act
        await using (var writeDbContext = _fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(source);
            writeDbContext.ExternalGameRecords.Add(record);

            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = _fixture.CreateDbContext();

        var persistedRecord = await readDbContext
            .ExternalGameRecords
            .SingleAsync();

        // Assert
        persistedRecord.DataSourceId.ShouldBe(source.Id);
        persistedRecord.ExternalId.ShouldBe("144542");
        persistedRecord.GameId.ShouldBeNull();
        persistedRecord.Status.ShouldBe(
            ExternalGameRecordStatus.Unlinked);
        persistedRecord.FirstSeenAt.ShouldBe(ObservedAt);
        persistedRecord.LastSeenAt.ShouldBe(ObservedAt);
        persistedRecord.SourceUpdatedAt.ShouldBeNull();
    }

    [Fact]
    public async Task SaveAndLoad_ShouldPersistCanonicalGameLink()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var source = CreateDataSource(
            code: "igdb",
            name: "IGDB");

        var game = new Game("Kitaria Fables");

        var record = new ExternalGameRecord(
            source.Id,
            "144542",
            ObservedAt);

        record.LinkToGame(game.Id);

        // Act
        await using (var writeDbContext = _fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(source);
            writeDbContext.Games.Add(game);
            writeDbContext.ExternalGameRecords.Add(record);

            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = _fixture.CreateDbContext();

        var persistedRecord = await readDbContext
            .ExternalGameRecords
            .SingleAsync();

        // Assert
        persistedRecord.GameId.ShouldBe(game.Id);
        persistedRecord.Status.ShouldBe(
            ExternalGameRecordStatus.Linked);
    }

    [Fact]
    public async Task SaveChanges_ShouldRejectDuplicateExternalIdWithinSameSource()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var source = CreateDataSource(
            code: "igdb",
            name: "IGDB");

        var firstRecord = new ExternalGameRecord(
            source.Id,
            "144542",
            ObservedAt);

        var duplicateRecord = new ExternalGameRecord(
            source.Id,
            " 144542 ",
            ObservedAt);

        await using var dbContext = _fixture.CreateDbContext();

        dbContext.DataSources.Add(source);

        dbContext.ExternalGameRecords.AddRange(
            firstRecord,
            duplicateRecord);

        // Act
        var action = async () =>
            await dbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveChanges_ShouldAllowSameExternalIdInDifferentSources()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var igdb = CreateDataSource(
            code: "igdb",
            name: "IGDB");

        var steam = CreateDataSource(
            code: "steam",
            name: "Steam");

        var igdbRecord = new ExternalGameRecord(
            igdb.Id,
            "620",
            ObservedAt);

        var steamRecord = new ExternalGameRecord(
            steam.Id,
            "620",
            ObservedAt);

        // Act
        await using (var writeDbContext = _fixture.CreateDbContext())
        {
            writeDbContext.DataSources.AddRange(
                igdb,
                steam);

            writeDbContext.ExternalGameRecords.AddRange(
                igdbRecord,
                steamRecord);

            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = _fixture.CreateDbContext();

        var records = await readDbContext
            .ExternalGameRecords
            .OrderBy(record => record.DataSourceId)
            .ToListAsync();

        // Assert
        records.Count.ShouldBe(2);
        records.ShouldAllBe(record =>
            record.ExternalId == "620");
    }

    [Fact]
    public async Task SaveChanges_ShouldRejectDuplicateDataSourceCode()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var firstSource = CreateDataSource(
            code: "IGDB",
            name: "IGDB");

        var duplicateSource = CreateDataSource(
            code: "igdb",
            name: "Another IGDB");

        await using var dbContext = _fixture.CreateDbContext();

        dbContext.DataSources.AddRange(
            firstSource,
            duplicateSource);

        // Act
        var action = async () =>
            await dbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveChanges_ShouldRestrictDeletingReferencedDataSource()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var source = CreateDataSource(
            code: "igdb",
            name: "IGDB");

        var record = new ExternalGameRecord(
            source.Id,
            "144542",
            ObservedAt);

        await using (var writeDbContext = _fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(source);
            writeDbContext.ExternalGameRecords.Add(record);

            await writeDbContext.SaveChangesAsync();
        }

        await using var deleteDbContext = _fixture.CreateDbContext();

        var persistedSource = await deleteDbContext
            .DataSources
            .SingleAsync();

        deleteDbContext.DataSources.Remove(persistedSource);

        // Act
        var action = async () =>
            await deleteDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveChanges_ShouldRestrictDeletingLinkedCanonicalGame()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var source = CreateDataSource(
            code: "igdb",
            name: "IGDB");

        var game = new Game("Kitaria Fables");

        var record = new ExternalGameRecord(
            source.Id,
            "144542",
            ObservedAt);

        record.LinkToGame(game.Id);

        await using (var writeDbContext = _fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(source);
            writeDbContext.Games.Add(game);
            writeDbContext.ExternalGameRecords.Add(record);

            await writeDbContext.SaveChangesAsync();
        }

        await using var deleteDbContext = _fixture.CreateDbContext();

        var persistedGame = await deleteDbContext
            .Games
            .SingleAsync();

        deleteDbContext.Games.Remove(persistedGame);

        // Act
        var action = async () =>
            await deleteDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    private static DataSource CreateDataSource(
        string code,
        string name)
    {
        var reliability = new SourceReliability(
            ReliabilityLevel.PublicDirect,
            "Dados obtidos diretamente de uma fonte pública.");

        return new DataSource(
            code,
            name,
            $"https://example.com/{code.ToLowerInvariant()}",
            reliability,
            attributionRequired: true);
    }
}