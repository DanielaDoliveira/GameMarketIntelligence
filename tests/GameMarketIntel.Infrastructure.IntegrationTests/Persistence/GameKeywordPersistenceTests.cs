using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence;

[Collection(PostgreSqlCollection.Name)]
public sealed class GameKeywordPersistenceTests(PostgreSqlFixture fixture)
{
    private static readonly DateTimeOffset ObservedAt = new(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task SaveAndLoad_ShouldPersistGameKeywordWithCompleteProvenance()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var keyword = new Keyword("Animal protagonist");

        var externalGameRecord = CreateExternalGameRecord(source, game, "144542");

        var externalKeywordRecord = CreateExternalKeywordRecord(source, keyword, "42");

        var association = new GameKeyword(game.Id, keyword.Id, externalGameRecord, externalKeywordRecord);

        await using (var writeDbContext = fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(source);
            writeDbContext.Games.Add(game);
            writeDbContext.Keywords.Add(keyword);
            writeDbContext.ExternalGameRecords.Add(externalGameRecord);
            writeDbContext.ExternalKeywordRecords.Add(externalKeywordRecord);
            writeDbContext.GameKeywords.Add(association);

            await writeDbContext.SaveChangesAsync();
        }

        // Act
        await using var readDbContext = fixture.CreateDbContext();

        var persistedAssociation = await readDbContext.GameKeywords.SingleAsync();

        // Assert
        persistedAssociation.GameId.ShouldBe(game.Id);
        persistedAssociation.KeywordId.ShouldBe(keyword.Id);
        persistedAssociation.ExternalGameRecordId.ShouldBe(externalGameRecord.Id);
        persistedAssociation.ExternalKeywordRecordId.ShouldBe(externalKeywordRecord.Id);
    }

    [Fact]
    public async Task SaveChanges_ShouldRejectDuplicateExternalContribution()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var keyword = new Keyword("Animal protagonist");

        var externalGameRecord = CreateExternalGameRecord(source, game, "144542");

        var externalKeywordRecord = CreateExternalKeywordRecord(source, keyword, "42");

        var firstAssociation = new GameKeyword(game.Id, keyword.Id, externalGameRecord, externalKeywordRecord);

        await using (var firstDbContext = fixture.CreateDbContext())
        {
            firstDbContext.DataSources.Add(source);
            firstDbContext.Games.Add(game);
            firstDbContext.Keywords.Add(keyword);
            firstDbContext.ExternalGameRecords.Add(externalGameRecord);
            firstDbContext.ExternalKeywordRecords.Add(externalKeywordRecord);
            firstDbContext.GameKeywords.Add(firstAssociation);

            await firstDbContext.SaveChangesAsync();
        }

        await using var duplicateDbContext = fixture.CreateDbContext();

        var persistedExternalGameRecord = await duplicateDbContext.ExternalGameRecords.SingleAsync();

        var persistedExternalKeywordRecord = await duplicateDbContext.ExternalKeywordRecords.SingleAsync();

        var duplicateAssociation = new GameKeyword(game.Id, keyword.Id, persistedExternalGameRecord,
            persistedExternalKeywordRecord);

        duplicateDbContext.GameKeywords.Add(duplicateAssociation);

        // Act
        var action = () => duplicateDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveChanges_ShouldAllowSameCanonicalKeywordFromDifferentExternalContributions()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var keyword = new Keyword("Animal protagonist");

        var firstExternalGameRecord = CreateExternalGameRecord(source, game, "144542");

        var secondExternalGameRecord = CreateExternalGameRecord(source, game, "398521");

        var firstExternalKeywordRecord = CreateExternalKeywordRecord(source, keyword, "42");

        var secondExternalKeywordRecord = CreateExternalKeywordRecord(source, keyword, "142");

        var firstAssociation =
            new GameKeyword(game.Id, keyword.Id, firstExternalGameRecord, firstExternalKeywordRecord);

        var secondAssociation =
            new GameKeyword(game.Id, keyword.Id, secondExternalGameRecord, secondExternalKeywordRecord);

        // Act
        await using (var dbContext = fixture.CreateDbContext())
        {
            dbContext.DataSources.Add(source);
            dbContext.Games.Add(game);
            dbContext.Keywords.Add(keyword);

            dbContext.ExternalGameRecords.AddRange(firstExternalGameRecord, secondExternalGameRecord);

            dbContext.ExternalKeywordRecords.AddRange(firstExternalKeywordRecord, secondExternalKeywordRecord);

            dbContext.GameKeywords.AddRange(firstAssociation, secondAssociation);

            await dbContext.SaveChangesAsync();
        }

        await using var readDbContext = fixture.CreateDbContext();

        var associationCount = await readDbContext.GameKeywords.CountAsync();

        // Assert
        associationCount.ShouldBe(2);
    }

    [Fact]
    public async Task DeleteExternalGameRecord_ShouldFail_WhenAssociationReferencesIt()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        await SeedAssociationAsync();

        await using var deleteDbContext = fixture.CreateDbContext();

        var persistedExternalGameRecord = await deleteDbContext.ExternalGameRecords.SingleAsync();

        deleteDbContext.ExternalGameRecords.Remove(persistedExternalGameRecord);

        // Act
        var action = () => deleteDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task DeleteExternalKeywordRecord_ShouldFail_WhenAssociationReferencesIt()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        await SeedAssociationAsync();

        await using var deleteDbContext = fixture.CreateDbContext();

        var persistedExternalKeywordRecord = await deleteDbContext.ExternalKeywordRecords.SingleAsync();

        deleteDbContext.ExternalKeywordRecords.Remove(persistedExternalKeywordRecord);

        // Act
        var action = () => deleteDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task DeleteKeyword_ShouldFail_WhenAssociationReferencesIt()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        await SeedAssociationAsync();

        await using var deleteDbContext = fixture.CreateDbContext();

        var persistedKeyword = await deleteDbContext.Keywords.SingleAsync();

        deleteDbContext.Keywords.Remove(persistedKeyword);

        // Act
        var action = () => deleteDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    private async Task SeedAssociationAsync()
    {
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var keyword = new Keyword("Animal protagonist");

        var externalGameRecord = CreateExternalGameRecord(source, game, "144542");

        var externalKeywordRecord = CreateExternalKeywordRecord(source, keyword, "42");

        var association = new GameKeyword(game.Id, keyword.Id, externalGameRecord, externalKeywordRecord);

        await using var dbContext = fixture.CreateDbContext();

        dbContext.DataSources.Add(source);
        dbContext.Games.Add(game);
        dbContext.Keywords.Add(keyword);
        dbContext.ExternalGameRecords.Add(externalGameRecord);
        dbContext.ExternalKeywordRecords.Add(externalKeywordRecord);
        dbContext.GameKeywords.Add(association);

        await dbContext.SaveChangesAsync();
    }

    private static ExternalGameRecord CreateExternalGameRecord(DataSource source, Game game, string externalId)
    {
        var externalGameRecord = new ExternalGameRecord(
            source.Id,
            externalId,
            ObservedAt);

        externalGameRecord.LinkToGame(game.Id);

        return externalGameRecord;
    }

    private static ExternalKeywordRecord CreateExternalKeywordRecord(DataSource source, Keyword keyword,
        string externalId)
    {
        var externalKeywordRecord = new ExternalKeywordRecord(source.Id, externalId, ObservedAt);

        externalKeywordRecord.LinkToKeyword(keyword.Id);

        return externalKeywordRecord;
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