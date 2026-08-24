using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence;

[Collection(PostgreSqlCollection.Name)]
public sealed class GameCollectionPersistenceTests
{
    private readonly PostgreSqlFixture _fixture;

    public GameCollectionPersistenceTests(PostgreSqlFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task SaveAndLoad_ShouldPersistGameCollectionWithProvenance()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var dataSource = CreateDataSource();

        var game = new Game("The Legend of Zelda: Ocarina of Time");
        var collection = new Collection("The Legend of Zelda");
        var observedAt = DateTimeOffset.UtcNow;

        var externalGameRecord = new ExternalGameRecord(
            dataSource.Id,
            "game-1",
            observedAt);

        var externalCollectionRecord = new ExternalCollectionRecord(
            dataSource.Id,
            "collection-1",
            observedAt);

        externalGameRecord.LinkToGame(game.Id);
        externalCollectionRecord.LinkToCollection(collection.Id);

        var gameCollection = new GameCollection(
            externalGameRecord,
            externalCollectionRecord);

        // Act
        await using (var writeDbContext = _fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(dataSource);
            writeDbContext.Games.Add(game);
            writeDbContext.Collections.Add(collection);
            writeDbContext.ExternalGameRecords.Add(externalGameRecord);
            writeDbContext.ExternalCollectionRecords.Add(externalCollectionRecord);
            writeDbContext.GameCollections.Add(gameCollection);

            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = _fixture.CreateDbContext();

        var persistedCollection = await readDbContext.Collections.SingleAsync();
        var persistedExternalCollectionRecord = await readDbContext.ExternalCollectionRecords.SingleAsync();
        var persistedGameCollection = await readDbContext.GameCollections.SingleAsync();

        // Assert
        persistedCollection.Id.ShouldBe(collection.Id);
        persistedCollection.Name.ShouldBe("The Legend of Zelda");
        persistedCollection.NormalizedName.ShouldBe("THE LEGEND OF ZELDA");

        persistedExternalCollectionRecord.DataSourceId.ShouldBe(dataSource.Id);
        persistedExternalCollectionRecord.ExternalId.ShouldBe("collection-1");
        persistedExternalCollectionRecord.CollectionId.ShouldBe(collection.Id);

        persistedGameCollection.GameId.ShouldBe(game.Id);
        persistedGameCollection.CollectionId.ShouldBe(collection.Id);
        persistedGameCollection.ExternalGameRecordId.ShouldBe(externalGameRecord.Id);
        persistedGameCollection.ExternalCollectionRecordId.ShouldBe(externalCollectionRecord.Id);
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