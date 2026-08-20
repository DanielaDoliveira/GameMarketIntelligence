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
    private static readonly DateTimeOffset ObservedAt =
        new(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);


    [Fact]
    public async Task SaveAndLoad_ShouldPersistGameKeywordWithProvenance()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var keyword = new Keyword("Animal protagonist");
        var externalRecord = CreateExternalRecord(source, game, "144542");

        var association = new GameKeyword(game.Id, keyword.Id, externalRecord);

        // Act
        await using (var writeDbContext = fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(source);
            writeDbContext.Games.Add(game);
            writeDbContext.Keywords.Add(keyword);
            writeDbContext.ExternalGameRecords.Add(externalRecord);
            writeDbContext.GameKeywords.Add(association);

            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = fixture.CreateDbContext();
        var persistedAssociation = await readDbContext.GameKeywords.SingleAsync();

        // Assert
        persistedAssociation.GameId.ShouldBe(game.Id);
        persistedAssociation.KeywordId.ShouldBe(keyword.Id);
        persistedAssociation.ExternalGameRecordId.ShouldBe(externalRecord.Id);
    }

    [Fact]
    public async Task SaveChanges_ShouldRejectDuplicateGameKeywordContribution()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var keyword = new Keyword("Animal protagonist");
        var externalRecord = CreateExternalRecord(source, game, "144542");

        var firstAssociation = new GameKeyword(game.Id, keyword.Id, externalRecord);

        var duplicateAssociation = new GameKeyword(game.Id, keyword.Id, externalRecord);

        await using var dbContext = fixture.CreateDbContext();

        dbContext.DataSources.Add(source);
        dbContext.Games.Add(game);
        dbContext.Keywords.Add(keyword);
        dbContext.ExternalGameRecords.Add(externalRecord);
        dbContext.GameKeywords.AddRange(firstAssociation, duplicateAssociation);

        // Act
        var action = async () => await dbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SaveChanges_ShouldAllowSameGameAndKeywordFromDifferentExternalRecords()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var keyword = new Keyword("Animal protagonist");

        var firstExternalRecord = CreateExternalRecord(source, game, "144542");

        var secondExternalRecord = CreateExternalRecord(source, game, "398521");

        var firstAssociation = new GameKeyword(game.Id, keyword.Id, firstExternalRecord);

        var secondAssociation = new GameKeyword(game.Id, keyword.Id, secondExternalRecord);

        // Act
        await using (var dbContext = fixture.CreateDbContext())
        {
            dbContext.DataSources.Add(source);
            dbContext.Games.Add(game);
            dbContext.Keywords.Add(keyword);
            dbContext.ExternalGameRecords.AddRange(firstExternalRecord, secondExternalRecord);
            dbContext.GameKeywords.AddRange(firstAssociation, secondAssociation);

            await dbContext.SaveChangesAsync();
        }

        await using var readDbContext = fixture.CreateDbContext();

        // Assert
        (await readDbContext.GameKeywords.CountAsync()).ShouldBe(2);
    }

    [Fact]
    public async Task SaveChanges_ShouldRestrictDeletingReferencedExternalGameRecord()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        await SeedAssociationAsync();

        await using var deleteDbContext = fixture.CreateDbContext();

        var persistedExternalRecord = await deleteDbContext.ExternalGameRecords.SingleAsync();

        deleteDbContext.ExternalGameRecords.Remove(persistedExternalRecord);

        // Act
        var action = async () => await deleteDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveChanges_ShouldRestrictDeletingReferencedKeyword()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        await SeedAssociationAsync();

        await using var deleteDbContext = fixture.CreateDbContext();

        var persistedKeyword = await deleteDbContext.Keywords.SingleAsync();

        deleteDbContext.Keywords.Remove(persistedKeyword);

        // Act
        var action = async () => await deleteDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    private async Task SeedAssociationAsync()
    {
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var keyword = new Keyword("Animal protagonist");
        var externalRecord = CreateExternalRecord(source, game, "144542");

        var association = new GameKeyword(game.Id, keyword.Id, externalRecord);

        await using var dbContext = fixture.CreateDbContext();

        dbContext.DataSources.Add(source);
        dbContext.Games.Add(game);
        dbContext.Keywords.Add(keyword);
        dbContext.ExternalGameRecords.Add(externalRecord);
        dbContext.GameKeywords.Add(association);

        await dbContext.SaveChangesAsync();
    }

    private static ExternalGameRecord CreateExternalRecord(DataSource source, Game game, string externalId)
    {
        var externalRecord = new ExternalGameRecord(source.Id, externalId, ObservedAt);

        externalRecord.LinkToGame(game.Id);

        return externalRecord;
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