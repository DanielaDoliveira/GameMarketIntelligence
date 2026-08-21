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
    private static readonly DateTimeOffset ObservedAt =
        new(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task SaveAndLoad_ShouldPersistGameGameModeWithCompleteProvenance()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var gameMode = new GameMode("Single player");

        var externalGameRecord = CreateExternalGameRecord(source, game, "144542");

        var externalGameModeRecord = CreateExternalGameModeRecord(source, gameMode, "1");

        var association = new GameGameMode(game.Id, gameMode.Id, externalGameRecord, externalGameModeRecord);

        await using (var writeDbContext = fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(source);
            writeDbContext.Games.Add(game);
            writeDbContext.GameModes.Add(gameMode);
            writeDbContext.ExternalGameRecords.Add(externalGameRecord);
            writeDbContext.ExternalGameModeRecords.Add(externalGameModeRecord);
            writeDbContext.GameGameModes.Add(association);

            await writeDbContext.SaveChangesAsync();
        }

        // Act
        await using var readDbContext = fixture.CreateDbContext();

        var persistedAssociation = await readDbContext.GameGameModes.SingleAsync();

        // Assert
        persistedAssociation.GameId.ShouldBe(game.Id);
        persistedAssociation.GameModeId.ShouldBe(gameMode.Id);
        persistedAssociation.ExternalGameRecordId.ShouldBe(externalGameRecord.Id);
        persistedAssociation.ExternalGameModeRecordId.ShouldBe(externalGameModeRecord.Id);
    }

    [Fact]
    public async Task SaveChanges_ShouldRejectDuplicateExternalContribution()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var gameMode = new GameMode("Single player");

        var externalGameRecord = CreateExternalGameRecord(source, game, "144542");

        var externalGameModeRecord = CreateExternalGameModeRecord(source, gameMode, "1");

        var firstAssociation = new GameGameMode(game.Id, gameMode.Id, externalGameRecord, externalGameModeRecord);

        await using (var firstDbContext = fixture.CreateDbContext())
        {
            firstDbContext.DataSources.Add(source);
            firstDbContext.Games.Add(game);
            firstDbContext.GameModes.Add(gameMode);
            firstDbContext.ExternalGameRecords.Add(externalGameRecord);
            firstDbContext.ExternalGameModeRecords.Add(externalGameModeRecord);
            firstDbContext.GameGameModes.Add(firstAssociation);

            await firstDbContext.SaveChangesAsync();
        }

        await using var duplicateDbContext = fixture.CreateDbContext();

        var persistedExternalGameRecord = await duplicateDbContext.ExternalGameRecords.SingleAsync();

        var persistedExternalGameModeRecord = await duplicateDbContext.ExternalGameModeRecords.SingleAsync();

        var duplicateAssociation = new GameGameMode(game.Id, gameMode.Id, persistedExternalGameRecord, persistedExternalGameModeRecord);

        duplicateDbContext.GameGameModes.Add(duplicateAssociation);

        // Act
        var action = () => duplicateDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveChanges_ShouldAllowSameCanonicalGameModeFromDifferentExternalContributions()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var gameMode = new GameMode("Single player");

        var firstExternalGameRecord = CreateExternalGameRecord(source, game, "144542");

        var secondExternalGameRecord = CreateExternalGameRecord(source, game, "398521");

        var firstExternalGameModeRecord = CreateExternalGameModeRecord(source, gameMode, "1");

        var secondExternalGameModeRecord = CreateExternalGameModeRecord(source, gameMode, "101");

        var firstAssociation = new GameGameMode(game.Id, gameMode.Id, firstExternalGameRecord, firstExternalGameModeRecord);

        var secondAssociation = new GameGameMode(game.Id, gameMode.Id, secondExternalGameRecord, secondExternalGameModeRecord);

        // Act
        await using (var dbContext = fixture.CreateDbContext())
        {
            dbContext.DataSources.Add(source);
            dbContext.Games.Add(game);
            dbContext.GameModes.Add(gameMode);

            dbContext.ExternalGameRecords.AddRange(firstExternalGameRecord, secondExternalGameRecord);

            dbContext.ExternalGameModeRecords.AddRange(firstExternalGameModeRecord, secondExternalGameModeRecord);

            dbContext.GameGameModes.AddRange(firstAssociation, secondAssociation);

            await dbContext.SaveChangesAsync();
        }

        await using var readDbContext = fixture.CreateDbContext();

        var associationCount = await readDbContext.GameGameModes.CountAsync();

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
    public async Task DeleteExternalGameModeRecord_ShouldFail_WhenAssociationReferencesIt()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        await SeedAssociationAsync();

        await using var deleteDbContext = fixture.CreateDbContext();

        var persistedExternalGameModeRecord = await deleteDbContext.ExternalGameModeRecords.SingleAsync();

        deleteDbContext.ExternalGameModeRecords.Remove(persistedExternalGameModeRecord);

        // Act
        var action = () => deleteDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task DeleteGameMode_ShouldFail_WhenAssociationReferencesIt()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        await SeedAssociationAsync();

        await using var deleteDbContext = fixture.CreateDbContext();

        var persistedGameMode = await deleteDbContext.GameModes.SingleAsync();

        deleteDbContext.GameModes.Remove(persistedGameMode);

        // Act
        var action = () => deleteDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    private async Task SeedAssociationAsync()
    {
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var gameMode = new GameMode("Single player");

        var externalGameRecord = CreateExternalGameRecord(source, game, "144542");

        var externalGameModeRecord = CreateExternalGameModeRecord(source, gameMode, "1");

        var association = new GameGameMode(game.Id, gameMode.Id, externalGameRecord, externalGameModeRecord);

        await using var dbContext = fixture.CreateDbContext();

        dbContext.DataSources.Add(source);
        dbContext.Games.Add(game);
        dbContext.GameModes.Add(gameMode);
        dbContext.ExternalGameRecords.Add(externalGameRecord);
        dbContext.ExternalGameModeRecords.Add(externalGameModeRecord);
        dbContext.GameGameModes.Add(association);

        await dbContext.SaveChangesAsync();
    }

    private static ExternalGameRecord CreateExternalGameRecord(DataSource source, Game game, string externalId)
    {
        var externalGameRecord = new ExternalGameRecord(source.Id, externalId, ObservedAt);

        externalGameRecord.LinkToGame(game.Id);

        return externalGameRecord;
    }

    private static ExternalGameModeRecord CreateExternalGameModeRecord(DataSource source, GameMode gameMode, string externalId)
    {
        var externalGameModeRecord = new ExternalGameModeRecord(source.Id, externalId, ObservedAt);

        externalGameModeRecord.LinkToGameMode(gameMode.Id);

        return externalGameModeRecord;
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