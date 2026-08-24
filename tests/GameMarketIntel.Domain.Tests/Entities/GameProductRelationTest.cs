using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public sealed class GameProductRelationTests
{
    [Fact]
    public void Constructor_ShouldCreateRelationWithProvenance()
    {
        // Arrange
        var dataSourceId = Guid.NewGuid();
        var sourceGame = new Game("The Legend of Zelda: Ocarina of Time 3D", productType: GameProductType.Remake);
        var targetGame = new Game("The Legend of Zelda: Ocarina of Time", productType: GameProductType.MainGame);

        var sourceExternalGameRecord = CreateLinkedExternalGameRecord(
            dataSourceId,
            "source-1",
            sourceGame.Id);

        var targetExternalGameRecord = CreateLinkedExternalGameRecord(
            dataSourceId,
            "target-1",
            targetGame.Id);

        // Act
        var relation = new GameProductRelation(
            sourceExternalGameRecord,
            targetExternalGameRecord,
            GameProductRelationType.RemakeOf);

        // Assert
        relation.Id.ShouldNotBe(Guid.Empty);
        relation.SourceGameId.ShouldBe(sourceGame.Id);
        relation.TargetGameId.ShouldBe(targetGame.Id);
        relation.ExternalSourceGameRecordId.ShouldBe(sourceExternalGameRecord.Id);
        relation.ExternalTargetGameRecordId.ShouldBe(targetExternalGameRecord.Id);
        relation.RelationType.ShouldBe(GameProductRelationType.RemakeOf);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenSourceExternalGameRecordIsNull()
    {
        // Arrange
        var dataSourceId = Guid.NewGuid();
        var targetGame = new Game("Target");

        var targetExternalGameRecord = CreateLinkedExternalGameRecord(
            dataSourceId,
            "target-1",
            targetGame.Id);

        // Act
        var action = () => new GameProductRelation(
            null!,
            targetExternalGameRecord,
            GameProductRelationType.RemakeOf);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenTargetExternalGameRecordIsNull()
    {
        // Arrange
        var dataSourceId = Guid.NewGuid();
        var sourceGame = new Game("Source");

        var sourceExternalGameRecord = CreateLinkedExternalGameRecord(
            dataSourceId,
            "source-1",
            sourceGame.Id);

        // Act
        var action = () => new GameProductRelation(
            sourceExternalGameRecord,
            null!,
            GameProductRelationType.RemakeOf);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenSourceExternalGameRecordIsNotLinked()
    {
        // Arrange
        var dataSourceId = Guid.NewGuid();
        var sourceExternalGameRecord = CreateExternalGameRecord(
            dataSourceId,
            "source-1");

        var targetGame = new Game("Target");

        var targetExternalGameRecord = CreateLinkedExternalGameRecord(
            dataSourceId,
            "target-1",
            targetGame.Id);

        // Act
        var action = () => new GameProductRelation(
            sourceExternalGameRecord,
            targetExternalGameRecord,
            GameProductRelationType.RemakeOf);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenTargetExternalGameRecordIsNotLinked()
    {
        // Arrange
        var dataSourceId = Guid.NewGuid();
        var sourceGame = new Game("Source");

        var sourceExternalGameRecord = CreateLinkedExternalGameRecord(
            dataSourceId,
            "source-1",
            sourceGame.Id);

        var targetExternalGameRecord = CreateExternalGameRecord(
            dataSourceId,
            "target-1");

        // Act
        var action = () => new GameProductRelation(
            sourceExternalGameRecord,
            targetExternalGameRecord,
            GameProductRelationType.RemakeOf);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenExternalGameRecordsBelongToDifferentDataSources()
    {
        // Arrange
        var sourceGame = new Game("Source");
        var targetGame = new Game("Target");

        var sourceExternalGameRecord = CreateLinkedExternalGameRecord(
            Guid.NewGuid(),
            "source-1",
            sourceGame.Id);

        var targetExternalGameRecord = CreateLinkedExternalGameRecord(
            Guid.NewGuid(),
            "target-1",
            targetGame.Id);

        // Act
        var action = () => new GameProductRelation(
            sourceExternalGameRecord,
            targetExternalGameRecord,
            GameProductRelationType.RemakeOf);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenSourceAndTargetResolveToTheSameGame()
    {
        // Arrange
        var dataSourceId = Guid.NewGuid();
        var game = new Game("Same Game");

        var sourceExternalGameRecord = CreateLinkedExternalGameRecord(
            dataSourceId,
            "source-1",
            game.Id);

        var targetExternalGameRecord = CreateLinkedExternalGameRecord(
            dataSourceId,
            "target-1",
            game.Id);

        // Act
        var action = () => new GameProductRelation(
            sourceExternalGameRecord,
            targetExternalGameRecord,
            GameProductRelationType.RemakeOf);

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
        new(dataSourceId, externalId, DateTimeOffset.UtcNow);
}