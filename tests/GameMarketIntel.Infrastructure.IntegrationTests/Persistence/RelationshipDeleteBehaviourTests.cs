using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence;

[Collection(PostgreSqlCollection.Name)]
public sealed class RelationshipDeleteBehaviorTests
{
    private readonly PostgreSqlFixture _fixture;

    public RelationshipDeleteBehaviorTests(PostgreSqlFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task SaveChanges_ShouldRejectDeletingCompany_WhenGameCompanyExists()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var setup = await CreateGameCompanyGraphAsync();

        await using var dbContext = _fixture.CreateDbContext();

        var company = await dbContext.Companies
            .SingleAsync(company => company.Id == setup.CompanyId);

        dbContext.Companies.Remove(company);

        // Act
        var action = async () => await dbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveChanges_ShouldRejectDeletingCollection_WhenGameCollectionExists()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var setup = await CreateGameCollectionGraphAsync();

        await using var dbContext = _fixture.CreateDbContext();

        var collection = await dbContext.Collections
            .SingleAsync(collection => collection.Id == setup.CollectionId);

        dbContext.Collections.Remove(collection);

        // Act
        var action = async () => await dbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveChanges_ShouldRejectDeletingGame_WhenProductRelationExists()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var setup = await CreateProductRelationGraphAsync();

        await using var dbContext = _fixture.CreateDbContext();

        var sourceGame = await dbContext.Games
            .SingleAsync(game => game.Id == setup.SourceGameId);

        dbContext.Games.Remove(sourceGame);

        // Act
        var action = async () => await dbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveChanges_ShouldRejectDeletingExternalGameRecord_WhenProductRelationExists()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var setup = await CreateProductRelationGraphAsync();

        await using var dbContext = _fixture.CreateDbContext();

        var externalGameRecord = await dbContext.ExternalGameRecords
            .SingleAsync(record => record.Id == setup.ExternalSourceGameRecordId);

        dbContext.ExternalGameRecords.Remove(externalGameRecord);

        // Act
        var action = async () => await dbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    private async Task<(Guid CompanyId, Guid GameId)> CreateGameCompanyGraphAsync()
    {
        var dataSource = CreateDataSource();
        var game = new Game("Company relationship test game");
        var company = new Company("Relationship Test Company");
        var observedAt = DateTimeOffset.UtcNow;

        var externalGameRecord = new ExternalGameRecord(
            dataSource.Id,
            "game-company-game",
            observedAt);

        var externalCompanyRecord = new ExternalCompanyRecord(
            dataSource.Id,
            "game-company-company",
            observedAt);

        externalGameRecord.LinkToGame(game.Id);
        externalCompanyRecord.LinkToCompany(company.Id);

        var gameCompany = new GameCompany(
            externalGameRecord,
            externalCompanyRecord,
            GameCompanyRole.Developer);

        await using var dbContext = _fixture.CreateDbContext();

        dbContext.DataSources.Add(dataSource);
        dbContext.Games.Add(game);
        dbContext.Companies.Add(company);
        dbContext.ExternalGameRecords.Add(externalGameRecord);
        dbContext.ExternalCompanyRecords.Add(externalCompanyRecord);
        dbContext.GameCompanies.Add(gameCompany);

        await dbContext.SaveChangesAsync();

        return (company.Id, game.Id);
    }

    private async Task<(Guid CollectionId, Guid GameId)> CreateGameCollectionGraphAsync()
    {
        var dataSource = CreateDataSource();
        var game = new Game("Collection relationship test game");
        var collection = new Collection("Relationship Test Collection");
        var observedAt = DateTimeOffset.UtcNow;

        var externalGameRecord = new ExternalGameRecord(
            dataSource.Id,
            "game-collection-game",
            observedAt);

        var externalCollectionRecord = new ExternalCollectionRecord(
            dataSource.Id,
            "game-collection-collection",
            observedAt);

        externalGameRecord.LinkToGame(game.Id);
        externalCollectionRecord.LinkToCollection(collection.Id);

        var gameCollection = new GameCollection(
            externalGameRecord,
            externalCollectionRecord);

        await using var dbContext = _fixture.CreateDbContext();

        dbContext.DataSources.Add(dataSource);
        dbContext.Games.Add(game);
        dbContext.Collections.Add(collection);
        dbContext.ExternalGameRecords.Add(externalGameRecord);
        dbContext.ExternalCollectionRecords.Add(externalCollectionRecord);
        dbContext.GameCollections.Add(gameCollection);

        await dbContext.SaveChangesAsync();

        return (collection.Id, game.Id);
    }

    private async Task<(Guid SourceGameId, Guid ExternalSourceGameRecordId)> CreateProductRelationGraphAsync()
    {
        var dataSource = CreateDataSource();

        var sourceGame = new Game(
            name: "Source relationship test game",
            productType: GameProductType.Remake);

        var targetGame = new Game(
            name: "Target relationship test game",
            productType: GameProductType.MainGame);

        var observedAt = DateTimeOffset.UtcNow;

        var sourceExternalGameRecord = new ExternalGameRecord(
            dataSource.Id,
            "product-relation-source",
            observedAt);

        var targetExternalGameRecord = new ExternalGameRecord(
            dataSource.Id,
            "product-relation-target",
            observedAt);

        sourceExternalGameRecord.LinkToGame(sourceGame.Id);
        targetExternalGameRecord.LinkToGame(targetGame.Id);

        var relation = new GameProductRelation(
            sourceExternalGameRecord,
            targetExternalGameRecord,
            GameProductRelationType.RemakeOf);

        await using var dbContext = _fixture.CreateDbContext();

        dbContext.DataSources.Add(dataSource);
        dbContext.Games.AddRange(sourceGame, targetGame);

        dbContext.ExternalGameRecords.AddRange(
            sourceExternalGameRecord,
            targetExternalGameRecord);

        dbContext.GameProductRelations.Add(relation);

        await dbContext.SaveChangesAsync();

        return (sourceGame.Id, sourceExternalGameRecord.Id);
    }

    private static DataSource CreateDataSource()
    {
        var reliability = new SourceReliability(
            ReliabilityLevel.PublicDirect,
            "Dados obtidos diretamente de uma fonte pública.");

        return new DataSource(
            code: $"test-{Guid.NewGuid():N}",
            name: "Relationship Test Source",
            url: "https://example.com",
            reliability,
            attributionRequired: false);
    }
}