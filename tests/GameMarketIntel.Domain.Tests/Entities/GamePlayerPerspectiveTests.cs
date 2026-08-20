using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public class GamePlayerPerspectiveTests
{
    [Fact]
    public void Constructor_ShouldInitializeGamePlayerPerspective()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var playerPerspectiveId = Guid.NewGuid();
        var externalGameRecordId = Guid.NewGuid();

        // Act
        var gamePlayerPerspective = new GamePlayerPerspective(gameId, playerPerspectiveId, externalGameRecordId);

        // Assert
        gamePlayerPerspective.GameId.ShouldBe(gameId);
        gamePlayerPerspective.PlayerPerspectiveId.ShouldBe(playerPerspectiveId);
        gamePlayerPerspective.ExternalGameRecordId.ShouldBe(externalGameRecordId);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenGameIdIsEmpty()
    {
        // Act
        var action = () => new GamePlayerPerspective(Guid.Empty, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenPlayerPerspectiveIdIsEmpty()
    {
        // Act
        var action = () => new GamePlayerPerspective(Guid.NewGuid(), Guid.Empty, Guid.NewGuid());

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenExternalGameRecordIdIsEmpty()
    {
        // Act
        var action = () => new GamePlayerPerspective(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }
}