using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public sealed class GameGameModeTests
{
    [Fact]
    public void Constructor_ShouldCreateGameGameMode_WhenValuesAreValid()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var gameModeId = Guid.NewGuid();
        var externalGameRecord = CreateLinkedExternalGameRecord(gameId);

        // Act
        var gameGameMode = new GameGameMode(gameId,gameModeId, externalGameRecord);

        // Assert
        gameGameMode.GameId.ShouldBe(gameId);
        gameGameMode.GameModeId.ShouldBe(gameModeId);
        gameGameMode.ExternalGameRecordId.ShouldBe(externalGameRecord.Id);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenGameIdIsEmpty()
    {
        // Arrange
        var gameModeId = Guid.NewGuid();
        var externalGameRecord = CreateLinkedExternalGameRecord(
            Guid.NewGuid());

        // Act
        var action = () => new GameGameMode(Guid.Empty, gameModeId, externalGameRecord);

        // Assert
        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("gameId");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenGameModeIdIsEmpty()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var externalGameRecord = CreateLinkedExternalGameRecord(gameId);

        // Act
        var action = () => new GameGameMode(gameId, Guid.Empty, externalGameRecord);

        // Assert
        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("gameModeId");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameRecordIsNull()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var gameModeId = Guid.NewGuid();

        // Act
        var action = () => new GameGameMode(gameId, gameModeId, null!);

        // Assert
        action.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("externalGameRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameRecordIsNotLinked()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var gameModeId = Guid.NewGuid();

        var externalGameRecord = new ExternalGameRecord(Guid.NewGuid(), "144542", DateTimeOffset.UtcNow);

        // Act
        var action = () => new GameGameMode(gameId, gameModeId, externalGameRecord);

        // Assert
        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalGameRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameRecordIsLinkedToDifferentGame()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var differentGameId = Guid.NewGuid();
        var gameModeId = Guid.NewGuid();

        var externalGameRecord =
            CreateLinkedExternalGameRecord(differentGameId);

        // Act
        var action = () => new GameGameMode(gameId, gameModeId, externalGameRecord);

        // Assert
        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalGameRecord");
    }

    private static ExternalGameRecord CreateLinkedExternalGameRecord(Guid gameId)
    {
        var externalGameRecord = new ExternalGameRecord(Guid.NewGuid(), "144542", DateTimeOffset.UtcNow);

        externalGameRecord.LinkToGame(gameId);

        return externalGameRecord;
    }
}