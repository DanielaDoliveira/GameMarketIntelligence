using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence;

[Collection(PostgreSqlCollection.Name)]
public sealed class ExternalKeywordRecordPersistenceTests
{
    private readonly PostgreSqlFixture _fixture;

    public ExternalKeywordRecordPersistenceTests(PostgreSqlFixture fixture)=> _fixture = fixture;
    

    [Fact]
    public async Task SaveAndLoad_ShouldPersistExternalKeywordRecord()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var dataSource = CreateDataSource();

        var observedAt = new DateTimeOffset(2026, 8, 20, 10, 0, 0, TimeSpan.Zero);

        var record = new ExternalKeywordRecord(dataSource.Id, "42", observedAt);

        // Act
        await using (var writeDbContext = _fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(dataSource);
            writeDbContext.ExternalKeywordRecords.Add(record);
            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = _fixture.CreateDbContext();
        var persistedRecord = await readDbContext.ExternalKeywordRecords.SingleAsync();

        // Assert
        persistedRecord.Id.ShouldBe(record.Id);
        persistedRecord.DataSourceId.ShouldBe(dataSource.Id);
        persistedRecord.ExternalId.ShouldBe("42");
        persistedRecord.KeywordId.ShouldBeNull();
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

        var firstRecord = new ExternalKeywordRecord(dataSource.Id, "42", observedAt);

        var duplicateRecord = new ExternalKeywordRecord(dataSource.Id, "42", observedAt);

        await using var dbContext = _fixture.CreateDbContext();
        dbContext.DataSources.Add(dataSource);
        dbContext.ExternalKeywordRecords.AddRange(firstRecord, duplicateRecord);

        // Act
        var action = () => dbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveAndLoad_ShouldPersistOptionalKeywordLink()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var dataSource = CreateDataSource();
        var keyword = new Keyword("Time loop");

        var record = new ExternalKeywordRecord(dataSource.Id, "42", DateTimeOffset.UtcNow);

        record.LinkToKeyword(keyword.Id);

        // Act
        await using (var writeDbContext = _fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(dataSource);
            writeDbContext.Keywords.Add(keyword);
            writeDbContext.ExternalKeywordRecords.Add(record);
            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = _fixture.CreateDbContext();
        var persistedRecord = await readDbContext.ExternalKeywordRecords.SingleAsync();

        // Assert
        persistedRecord.KeywordId.ShouldBe(keyword.Id);
    }

    [Fact]
    public async Task DeleteKeyword_ShouldFail_WhenExternalKeywordRecordReferencesIt()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var dataSource = CreateDataSource();
        var keyword = new Keyword("Time loop");

        var record = new ExternalKeywordRecord(dataSource.Id, "42", DateTimeOffset.UtcNow);

        record.LinkToKeyword(keyword.Id);

        await using (var setupDbContext = _fixture.CreateDbContext())
        {
            setupDbContext.DataSources.Add(dataSource);
            setupDbContext.Keywords.Add(keyword);
            setupDbContext.ExternalKeywordRecords.Add(record);
            await setupDbContext.SaveChangesAsync();
        }

        await using var deleteDbContext = _fixture.CreateDbContext();
        var persistedKeyword = await deleteDbContext.Keywords .SingleAsync();

        deleteDbContext.Keywords.Remove(persistedKeyword);

        // Act
        var action = () => deleteDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    private static DataSource CreateDataSource()
    {
        var reliability = new SourceReliability(ReliabilityLevel.PublicDirect, "Dados obtidos diretamente de uma fonte pública.");

        return new DataSource(
            code: "igdb",
            name: "IGDB",
            url: "https://www.igdb.com",
            reliability,
            attributionRequired: true);
    }
}