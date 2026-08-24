using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public sealed class GameCollectionTests
{
    [Fact]
    public void Constructor_ShouldCreateGameCollectionWithProvenance()
    {
        // Arrange
        var dataSourceId = Guid.NewGuid();
        var game = new Game("The Legend of Zelda: Ocarina of Time");
        var collection = new Collection("The Legend of Zelda");

        var externalGameRecord = CreateLinkedExternalGameRecord(
            dataSourceId,
            "game-1",
            game.Id);

        var externalCollectionRecord = CreateLinkedExternalCollectionRecord(
            dataSourceId,
            "collection-1",
            collection.Id);

        // Act
        var gameCollection = new GameCollection(
            externalGameRecord,
            externalCollectionRecord);

        // Assert
        gameCollection.GameId.ShouldBe(game.Id);
        gameCollection.CollectionId.ShouldBe(collection.Id);
        gameCollection.ExternalGameRecordId.ShouldBe(externalGameRecord.Id);
        gameCollection.ExternalCollectionRecordId.ShouldBe(externalCollectionRecord.Id);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenExternalGameRecordIsNull()
    {
        // Arrange
        var collection = new Collection("The Legend of Zelda");

        var externalCollectionRecord = CreateLinkedExternalCollectionRecord(
            Guid.NewGuid(),
            "collection-1",
            collection.Id);

        // Act
        var action = () => new GameCollection(
            null!,
            externalCollectionRecord);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenExternalCollectionRecordIsNull()
    {
        // Arrange
        var game = new Game("The Legend of Zelda: Ocarina of Time");

        var externalGameRecord = CreateLinkedExternalGameRecord(
            Guid.NewGuid(),
            "game-1",
            game.Id);

        // Act
        var action = () => new GameCollection(
            externalGameRecord,
            null!);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenExternalGameRecordIsNotLinked()
    {
        // Arrange
        var dataSourceId = Guid.NewGuid();
        var collection = new Collection("The Legend of Zelda");

        var externalGameRecord = CreateExternalGameRecord(
            dataSourceId,
            "game-1");

        var externalCollectionRecord = CreateLinkedExternalCollectionRecord(
            dataSourceId,
            "collection-1",
            collection.Id);

        // Act
        var action = () => new GameCollection(
            externalGameRecord,
            externalCollectionRecord);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenExternalCollectionRecordIsNotLinked()
    {
        // Arrange
        var dataSourceId = Guid.NewGuid();
        var game = new Game("The Legend of Zelda: Ocarina of Time");

        var externalGameRecord = CreateLinkedExternalGameRecord(
            dataSourceId,
            "game-1",
            game.Id);

        var externalCollectionRecord = CreateExternalCollectionRecord(
            dataSourceId,
            "collection-1");

        // Act
        var action = () => new GameCollection(
            externalGameRecord,
            externalCollectionRecord);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenExternalRecordsBelongToDifferentDataSources()
    {
        // Arrange
        var game = new Game("The Legend of Zelda: Ocarina of Time");
        var collection = new Collection("The Legend of Zelda");

        var externalGameRecord = CreateLinkedExternalGameRecord(
            Guid.NewGuid(),
            "game-1",
            game.Id);

        var externalCollectionRecord = CreateLinkedExternalCollectionRecord(
            Guid.NewGuid(),
            "collection-1",
            collection.Id);

        // Act
        var action = () => new GameCollection(
            externalGameRecord,
            externalCollectionRecord);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    private static ExternalGameRecord CreateLinkedExternalGameRecord(Guid dataSourceId, string externalId, Guid gameId)
    {
        var record = CreateExternalGameRecord(dataSourceId, externalId);
        record.LinkToGame(gameId);

        return record;
    }

    private static ExternalGameRecord CreateExternalGameRecord(Guid dataSourceId, string externalId) =>
        new(
            dataSourceId,
            externalId,
            DateTimeOffset.UtcNow);

    private static ExternalCollectionRecord CreateLinkedExternalCollectionRecord(Guid dataSourceId, string externalId, Guid collectionId)
    {
        var record = CreateExternalCollectionRecord(dataSourceId, externalId);
        record.LinkToCollection(collectionId);

        return record;
    }

    private static ExternalCollectionRecord CreateExternalCollectionRecord(Guid dataSourceId, string externalId) =>
        new(
            dataSourceId,
            externalId,
            DateTimeOffset.UtcNow);
}