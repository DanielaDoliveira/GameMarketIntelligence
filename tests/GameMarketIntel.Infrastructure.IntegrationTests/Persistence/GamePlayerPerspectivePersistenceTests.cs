using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence;

[Collection(PostgreSqlCollection.Name)]
public sealed class GamePlayerPerspectivePersistenceTests(PostgreSqlFixture fixture)
{
    private static readonly DateTimeOffset ObservedAt = new(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);


    [Fact]
    public async Task SaveAndLoad_ShouldPersistGamePlayerPerspectiveWithProvenance()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var playerPerspective = new PlayerPerspective("Bird view");
        var externalRecord = CreateExternalRecord(source, game, "144542");

        var association = new GamePlayerPerspective(game.Id, playerPerspective.Id, externalRecord);

        // Act
        await using (var writeDbContext = fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(source);
            writeDbContext.Games.Add(game);
            writeDbContext.PlayerPerspectives.Add(playerPerspective);
            writeDbContext.ExternalGameRecords.Add(externalRecord);
            writeDbContext.GamePlayerPerspectives.Add(association);

            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = fixture.CreateDbContext();
        var persistedAssociation = await readDbContext.GamePlayerPerspectives.SingleAsync();

        // Assert
        persistedAssociation.GameId.ShouldBe(game.Id);
        persistedAssociation.PlayerPerspectiveId.ShouldBe(playerPerspective.Id); persistedAssociation.ExternalGameRecordId.ShouldBe(externalRecord.Id);
    }

    [Fact]
    public async Task SaveChanges_ShouldRejectDuplicateGamePlayerPerspectiveContribution()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var playerPerspective = new PlayerPerspective("Bird view");
        var externalRecord = CreateExternalRecord(source, game, "144542");

        var firstAssociation = new GamePlayerPerspective(game.Id, playerPerspective.Id, externalRecord);

        var duplicateAssociation = new GamePlayerPerspective(game.Id, playerPerspective.Id, externalRecord);

        await using var dbContext = fixture.CreateDbContext();

        dbContext.DataSources.Add(source);
        dbContext.Games.Add(game);
        dbContext.PlayerPerspectives.Add(playerPerspective);
        dbContext.ExternalGameRecords.Add(externalRecord);
        dbContext.GamePlayerPerspectives.AddRange(firstAssociation, duplicateAssociation);

        // Act
        var action = async () => await dbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SaveChanges_ShouldAllowSameGameAndPerspectiveFromDifferentExternalRecords()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var playerPerspective = new PlayerPerspective("Bird view");

        var firstExternalRecord = CreateExternalRecord(
            source,
            game,
            "144542");

        var secondExternalRecord = CreateExternalRecord(source, game, "398521");

        var firstAssociation = new GamePlayerPerspective(game.Id, playerPerspective.Id, firstExternalRecord);

        var secondAssociation = new GamePlayerPerspective(game.Id, playerPerspective.Id, secondExternalRecord);

        // Act
        await using (var dbContext = fixture.CreateDbContext())
        {
            dbContext.DataSources.Add(source);
            dbContext.Games.Add(game);
            dbContext.PlayerPerspectives.Add(playerPerspective);
            dbContext.ExternalGameRecords.AddRange(firstExternalRecord, secondExternalRecord);
            dbContext.GamePlayerPerspectives.AddRange(firstAssociation, secondAssociation);

            await dbContext.SaveChangesAsync();
        }

        await using var readDbContext = fixture.CreateDbContext();

        // Assert
        (await readDbContext.GamePlayerPerspectives.CountAsync()).ShouldBe(2);
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
    public async Task SaveChanges_ShouldRestrictDeletingReferencedPlayerPerspective()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        await SeedAssociationAsync();

        await using var deleteDbContext = fixture.CreateDbContext();

        var persistedPlayerPerspective = await deleteDbContext.PlayerPerspectives.SingleAsync();

        deleteDbContext.PlayerPerspectives.Remove(persistedPlayerPerspective);

        // Act
        var action = async () => await deleteDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    private async Task SeedAssociationAsync()
    {
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var playerPerspective = new PlayerPerspective("Bird view");
        var externalRecord = CreateExternalRecord(source, game, "144542");

        var association = new GamePlayerPerspective(game.Id, playerPerspective.Id, externalRecord);
        await using var dbContext = fixture.CreateDbContext();

        dbContext.DataSources.Add(source);
        dbContext.Games.Add(game);
        dbContext.PlayerPerspectives.Add(playerPerspective);
        dbContext.ExternalGameRecords.Add(externalRecord);
        dbContext.GamePlayerPerspectives.Add(association);

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