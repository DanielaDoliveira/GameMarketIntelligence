using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public class GameGameModeTests
{
    [Fact]
    public void Constructor_ShouldInitializeGameGameMode()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var gameModeId = Guid.NewGuid();
        var externalGameRecordId = Guid.NewGuid();

        // Act
        var gameGameMode = new GameGameMode(gameId, gameModeId, externalGameRecordId);

        // Assert
        gameGameMode.GameId.ShouldBe(gameId);
        gameGameMode.GameModeId.ShouldBe(gameModeId);
        gameGameMode.ExternalGameRecordId.ShouldBe(externalGameRecordId);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenGameIdIsEmpty()
    {
        // Act
        var action = () => new GameGameMode(Guid.Empty, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenGameModeIdIsEmpty()
    {
        // Act
        var action = () => new GameGameMode(Guid.NewGuid(), Guid.Empty, Guid.NewGuid());

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenExternalGameRecordIdIsEmpty()
    {
        // Act
        var action = () => new GameGameMode(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }
}