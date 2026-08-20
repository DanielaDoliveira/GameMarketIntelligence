using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public sealed class GamePlayerPerspectiveTests
{
    [Fact]
    public void Constructor_ShouldCreateGamePlayerPerspective_WhenValuesAreValid()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var playerPerspectiveId = Guid.NewGuid();
        var externalGameRecord = CreateLinkedExternalGameRecord(gameId);

        // Act
        var association = new GamePlayerPerspective(gameId, playerPerspectiveId, externalGameRecord);

        // Assert
        association.GameId.ShouldBe(gameId);
        association.PlayerPerspectiveId.ShouldBe(playerPerspectiveId);
        association.ExternalGameRecordId.ShouldBe(externalGameRecord.Id);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenGameIdIsEmpty()
    {
        // Arrange
        var playerPerspectiveId = Guid.NewGuid();
        var externalGameRecord = CreateLinkedExternalGameRecord(Guid.NewGuid());

        // Act
        var action = () => new GamePlayerPerspective(Guid.Empty, playerPerspectiveId, externalGameRecord);

        // Assert
        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("gameId");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenPlayerPerspectiveIdIsEmpty()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var externalGameRecord = CreateLinkedExternalGameRecord(gameId);

        // Act
        var action = () => new GamePlayerPerspective(gameId, Guid.Empty, externalGameRecord);

        // Assert
        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("playerPerspectiveId");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameRecordIsNull()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var playerPerspectiveId = Guid.NewGuid();

        // Act
        var action = () => new GamePlayerPerspective(gameId, playerPerspectiveId, null!);

        // Assert
        action.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("externalGameRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameRecordIsNotLinked()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var playerPerspectiveId = Guid.NewGuid();

        var externalGameRecord = new ExternalGameRecord(Guid.NewGuid(), "144542", DateTimeOffset.UtcNow);

        // Act
        var action = () => new GamePlayerPerspective(gameId, playerPerspectiveId, externalGameRecord);

        // Assert
        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalGameRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameRecordIsLinkedToDifferentGame()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var differentGameId = Guid.NewGuid();
        var playerPerspectiveId = Guid.NewGuid();

        var externalGameRecord =
            CreateLinkedExternalGameRecord(differentGameId);

        // Act
        var action = () => new GamePlayerPerspective(gameId, playerPerspectiveId, externalGameRecord);

        // Assert
        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalGameRecord");
    }

    private static ExternalGameRecord CreateLinkedExternalGameRecord(
        Guid gameId)
    {
        var externalGameRecord = new ExternalGameRecord(Guid.NewGuid(), "144542", DateTimeOffset.UtcNow);

        externalGameRecord.LinkToGame(gameId);

        return externalGameRecord;
    }
}