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
    public async Task SaveAndLoad_ShouldPersistGamePlayerPerspectiveWithCompleteProvenance()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var perspective = new PlayerPerspective("Third person");

        var externalGameRecord = CreateExternalGameRecord(source, game, "144542");

        var externalPerspectiveRecord = CreateExternalPlayerPerspectiveRecord(source, perspective, "1");

        var association = new GamePlayerPerspective(game.Id, perspective.Id, externalGameRecord, externalPerspectiveRecord);

        await using (var writeDbContext = fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(source);
            writeDbContext.Games.Add(game);
            writeDbContext.PlayerPerspectives.Add(perspective);
            writeDbContext.ExternalGameRecords.Add(externalGameRecord);
            writeDbContext.ExternalPlayerPerspectiveRecords.Add(externalPerspectiveRecord);
            writeDbContext.GamePlayerPerspectives.Add(association);

            await writeDbContext.SaveChangesAsync();
        }

        // Act
        await using var readDbContext = fixture.CreateDbContext();

        var persistedAssociation = await readDbContext.GamePlayerPerspectives.SingleAsync();

        // Assert
        persistedAssociation.GameId.ShouldBe(game.Id);
        persistedAssociation.PlayerPerspectiveId.ShouldBe(perspective.Id);
        persistedAssociation.ExternalGameRecordId.ShouldBe(externalGameRecord.Id);
        persistedAssociation.ExternalPlayerPerspectiveRecordId.ShouldBe(externalPerspectiveRecord.Id);
    }

    [Fact]
    public async Task SaveChanges_ShouldRejectDuplicateExternalContribution()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var perspective = new PlayerPerspective("Third person");

        var externalGameRecord = CreateExternalGameRecord(source, game, "144542");

        var externalPerspectiveRecord = CreateExternalPlayerPerspectiveRecord(source, perspective, "1");

        var firstAssociation = new GamePlayerPerspective(game.Id, perspective.Id, externalGameRecord, externalPerspectiveRecord);

        await using (var firstDbContext = fixture.CreateDbContext())
        {
            firstDbContext.DataSources.Add(source);
            firstDbContext.Games.Add(game);
            firstDbContext.PlayerPerspectives.Add(perspective);
            firstDbContext.ExternalGameRecords.Add(externalGameRecord);
            firstDbContext.ExternalPlayerPerspectiveRecords.Add(externalPerspectiveRecord);
            firstDbContext.GamePlayerPerspectives.Add(firstAssociation);

            await firstDbContext.SaveChangesAsync();
        }

        await using var duplicateDbContext = fixture.CreateDbContext();

        var persistedExternalGameRecord = await duplicateDbContext.ExternalGameRecords.SingleAsync();

        var persistedExternalPerspectiveRecord = await duplicateDbContext.ExternalPlayerPerspectiveRecords.SingleAsync();

        var duplicateAssociation = new GamePlayerPerspective(game.Id, perspective.Id, persistedExternalGameRecord, persistedExternalPerspectiveRecord);

        duplicateDbContext.GamePlayerPerspectives.Add(duplicateAssociation);

        // Act
        var action = () => duplicateDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveChanges_ShouldAllowSameCanonicalPerspectiveFromDifferentExternalContributions()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var perspective = new PlayerPerspective("Third person");

        var firstExternalGameRecord = CreateExternalGameRecord(source, game, "144542");

        var secondExternalGameRecord = CreateExternalGameRecord(source, game, "398521");

        var firstExternalPerspectiveRecord = CreateExternalPlayerPerspectiveRecord(source, perspective, "1");

        var secondExternalPerspectiveRecord = CreateExternalPlayerPerspectiveRecord(source, perspective, "101");

        var firstAssociation = new GamePlayerPerspective(game.Id, perspective.Id, firstExternalGameRecord, firstExternalPerspectiveRecord);

        var secondAssociation = new GamePlayerPerspective(game.Id, perspective.Id, secondExternalGameRecord, secondExternalPerspectiveRecord);

        // Act
        await using (var dbContext = fixture.CreateDbContext())
        {
            dbContext.DataSources.Add(source);
            dbContext.Games.Add(game);
            dbContext.PlayerPerspectives.Add(perspective);

            dbContext.ExternalGameRecords.AddRange(firstExternalGameRecord, secondExternalGameRecord);

            dbContext.ExternalPlayerPerspectiveRecords.AddRange(firstExternalPerspectiveRecord, secondExternalPerspectiveRecord);

            dbContext.GamePlayerPerspectives.AddRange(firstAssociation, secondAssociation);

            await dbContext.SaveChangesAsync();
        }

        await using var readDbContext = fixture.CreateDbContext();

        var associationCount = await readDbContext.GamePlayerPerspectives.CountAsync();

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
    public async Task DeleteExternalPlayerPerspectiveRecord_ShouldFail_WhenAssociationReferencesIt()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        await SeedAssociationAsync();

        await using var deleteDbContext = fixture.CreateDbContext();

        var persistedExternalPerspectiveRecord = await deleteDbContext
            .ExternalPlayerPerspectiveRecords
            .SingleAsync();

        deleteDbContext.ExternalPlayerPerspectiveRecords.Remove(persistedExternalPerspectiveRecord);

        // Act
        var action = () => deleteDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task DeletePlayerPerspective_ShouldFail_WhenAssociationReferencesIt()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        await SeedAssociationAsync();

        await using var deleteDbContext = fixture.CreateDbContext();

        var persistedPerspective = await deleteDbContext.PlayerPerspectives.SingleAsync();

        deleteDbContext.PlayerPerspectives.Remove(persistedPerspective);

        // Act
        var action = () => deleteDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    private async Task SeedAssociationAsync()
    {
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var perspective = new PlayerPerspective("Third person");

        var externalGameRecord = CreateExternalGameRecord(source, game, "144542");

        var externalPerspectiveRecord = CreateExternalPlayerPerspectiveRecord(source, perspective, "1");

        var association = new GamePlayerPerspective(game.Id, perspective.Id, externalGameRecord, externalPerspectiveRecord);

        await using var dbContext = fixture.CreateDbContext();

        dbContext.DataSources.Add(source);
        dbContext.Games.Add(game);
        dbContext.PlayerPerspectives.Add(perspective);
        dbContext.ExternalGameRecords.Add(externalGameRecord);
        dbContext.ExternalPlayerPerspectiveRecords.Add(externalPerspectiveRecord);
        dbContext.GamePlayerPerspectives.Add(association);

        await dbContext.SaveChangesAsync();
    }

    private static ExternalGameRecord CreateExternalGameRecord(DataSource source, Game game, string externalId)
    {
        var externalGameRecord = new ExternalGameRecord(source.Id, externalId, ObservedAt);

        externalGameRecord.LinkToGame(game.Id);

        return externalGameRecord;
    }

    private static ExternalPlayerPerspectiveRecord CreateExternalPlayerPerspectiveRecord(DataSource source, PlayerPerspective perspective, string externalId)
    {
        var externalPerspectiveRecord = new ExternalPlayerPerspectiveRecord(source.Id, externalId, ObservedAt);

        externalPerspectiveRecord.LinkToPlayerPerspective(perspective.Id);

        return externalPerspectiveRecord;
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