using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence;

[Collection(PostgreSqlCollection.Name)]
public sealed class GameGameModePersistenceTests(PostgreSqlFixture fixture)
{
    private static readonly DateTimeOffset ObservedAt = new(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);


    [Fact]
    public async Task SaveAndLoad_ShouldPersistGameGameModeWithProvenance()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var gameMode = new GameMode("Single player");
        var externalRecord = CreateExternalRecord(source, game, "144542");

        var gameGameMode = new GameGameMode(game.Id, gameMode.Id, externalRecord);

        // Act
        await using (var writeDbContext = fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(source);
            writeDbContext.Games.Add(game);
            writeDbContext.GameModes.Add(gameMode);
            writeDbContext.ExternalGameRecords.Add(externalRecord);
            writeDbContext.GameGameModes.Add(gameGameMode);

            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = fixture.CreateDbContext();
        var persistedGameGameMode = await readDbContext.GameGameModes.SingleAsync();

        // Assert
        persistedGameGameMode.GameId.ShouldBe(game.Id);
        persistedGameGameMode.GameModeId.ShouldBe(gameMode.Id);
        persistedGameGameMode.ExternalGameRecordId.ShouldBe(externalRecord.Id);
    }

    [Fact]
    public async Task SaveChanges_ShouldRejectDuplicateGameGameModeContribution()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var gameMode = new GameMode("Single player");
        var externalRecord = CreateExternalRecord(source, game, "144542");

        var firstAssociation = new GameGameMode(game.Id, gameMode.Id, externalRecord);

        var duplicateAssociation = new GameGameMode(game.Id, gameMode.Id, externalRecord);

        await using var dbContext = fixture.CreateDbContext();

        dbContext.DataSources.Add(source);
        dbContext.Games.Add(game);
        dbContext.GameModes.Add(gameMode);
        dbContext.ExternalGameRecords.Add(externalRecord);
        dbContext.GameGameModes.AddRange(firstAssociation, duplicateAssociation);

        // Act
        var action = async () => await dbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SaveChanges_ShouldAllowSameGameAndGameModeFromDifferentExternalRecords()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var gameMode = new GameMode("Single player");

        var firstExternalRecord = CreateExternalRecord(source, game, "144542");

        var secondExternalRecord = CreateExternalRecord(source, game, "398521");

        var firstAssociation = new GameGameMode(game.Id, gameMode.Id, firstExternalRecord);

        var secondAssociation = new GameGameMode(game.Id, gameMode.Id, secondExternalRecord);

        // Act
        await using (var dbContext = fixture.CreateDbContext())
        {
            dbContext.DataSources.Add(source);
            dbContext.Games.Add(game);
            dbContext.GameModes.Add(gameMode);
            dbContext.ExternalGameRecords.AddRange(firstExternalRecord, secondExternalRecord);
            dbContext.GameGameModes.AddRange(firstAssociation, secondAssociation);

            await dbContext.SaveChangesAsync();
        }

        await using var readDbContext = fixture.CreateDbContext();

        // Assert
        (await readDbContext.GameGameModes.CountAsync()).ShouldBe(2);
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
    public async Task SaveChanges_ShouldRestrictDeletingReferencedGameMode()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        await SeedAssociationAsync();

        await using var deleteDbContext = fixture.CreateDbContext();

        var persistedGameMode = await deleteDbContext.GameModes.SingleAsync();

        deleteDbContext.GameModes.Remove(persistedGameMode);

        // Act
        var action = async () => await deleteDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    private async Task SeedAssociationAsync()
    {
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var gameMode = new GameMode("Single player");
        var externalRecord = CreateExternalRecord(source, game, "144542");

        var gameGameMode = new GameGameMode(game.Id, gameMode.Id, externalRecord);

        await using var dbContext = fixture.CreateDbContext();

        dbContext.DataSources.Add(source);
        dbContext.Games.Add(game);
        dbContext.GameModes.Add(gameMode);
        dbContext.ExternalGameRecords.Add(externalRecord);
        dbContext.GameGameModes.Add(gameGameMode);

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