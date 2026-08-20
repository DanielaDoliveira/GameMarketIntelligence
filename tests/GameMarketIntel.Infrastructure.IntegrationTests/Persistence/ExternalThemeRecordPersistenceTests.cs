using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence;

[Collection(PostgreSqlCollection.Name)]
public sealed class ExternalThemeRecordPersistenceTests
{
    private readonly PostgreSqlFixture _fixture;

    public ExternalThemeRecordPersistenceTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SaveAndLoad_ShouldPersistExternalThemeRecord()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var dataSource = CreateDataSource();

        var observedAt = new DateTimeOffset(
            2026, 8, 20, 10, 0, 0, TimeSpan.Zero);

        var record = new ExternalThemeRecord(dataSource.Id, "19", observedAt);

        // Act
        await using (var writeDbContext = _fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(dataSource);
            writeDbContext.ExternalThemeRecords.Add(record);
            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = _fixture.CreateDbContext();
        var persistedRecord = await readDbContext.ExternalThemeRecords.SingleAsync();

        // Assert
        persistedRecord.Id.ShouldBe(record.Id);
        persistedRecord.DataSourceId.ShouldBe(dataSource.Id);
        persistedRecord.ExternalId.ShouldBe("19");
        persistedRecord.ThemeId.ShouldBeNull();
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

        var firstRecord = new ExternalThemeRecord(dataSource.Id, "19", observedAt);

        var duplicateRecord = new ExternalThemeRecord(dataSource.Id, "19", observedAt);

        await using var dbContext = _fixture.CreateDbContext();
        dbContext.DataSources.Add(dataSource); 
        dbContext.ExternalThemeRecords.AddRange(firstRecord, duplicateRecord);

        // Act
        var action = () => dbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveAndLoad_ShouldPersistOptionalThemeLink()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var dataSource = CreateDataSource();
        var theme = new Theme("Horror");

        var record = new ExternalThemeRecord(dataSource.Id, "19", DateTimeOffset.UtcNow);

        record.LinkToTheme(theme.Id);

        // Act
        await using (var writeDbContext = _fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(dataSource);
            writeDbContext.Themes.Add(theme);
            writeDbContext.ExternalThemeRecords.Add(record);
            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = _fixture.CreateDbContext();
        var persistedRecord = await readDbContext.ExternalThemeRecords .SingleAsync();

        // Assert
        persistedRecord.ThemeId.ShouldBe(theme.Id);
    }

    [Fact]
    public async Task DeleteTheme_ShouldFail_WhenExternalThemeRecordReferencesIt()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var dataSource = CreateDataSource();
        var theme = new Theme("Horror");

        var record = new ExternalThemeRecord(dataSource.Id, "19", DateTimeOffset.UtcNow);

        record.LinkToTheme(theme.Id);

        await using (var setupDbContext = _fixture.CreateDbContext())
        {
            setupDbContext.DataSources.Add(dataSource);
            setupDbContext.Themes.Add(theme);
            setupDbContext.ExternalThemeRecords.Add(record);
            await setupDbContext.SaveChangesAsync();
        }

        await using var deleteDbContext = _fixture.CreateDbContext();
        var persistedTheme = await deleteDbContext.Themes.SingleAsync();

        deleteDbContext.Themes.Remove(persistedTheme);

        // Act
        var action = () => deleteDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    private static DataSource CreateDataSource()
    {
        var reliability = new SourceReliability(ReliabilityLevel.PublicDirect, "Dados obtidos diretamente de uma fonte pública.");

        return new DataSource(code: "igdb", name: "IGDB", url: "https://www.igdb.com", reliability, attributionRequired: true);
    }
}