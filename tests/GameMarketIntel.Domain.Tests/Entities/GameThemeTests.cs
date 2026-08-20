using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public class GameThemeTests
{
    [Fact]
    public void Constructor_ShouldInitializeGameTheme()
    {
        //Arrange
        var gameId = Guid.NewGuid();
        var themeId = Guid.NewGuid();
        var externalGameRecordId = Guid.NewGuid();

        // Act
        var gameTheme = new GameTheme(gameId, themeId, externalGameRecordId);
        
        
        // Assert
        gameTheme.GameId.ShouldBe(gameId);
        gameTheme.ThemeId.ShouldBe(themeId);
        gameTheme.ExternalGameRecordId.ShouldBe(externalGameRecordId);
    }
    
    [Fact]
    public void Constructor_ShouldThrowException_WhenGameIdIsEmpty()
    {
        // Act
        var action = () => new GameTheme(Guid.Empty, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        action.ShouldThrow<ArgumentException>();
    }
    
    [Fact]
    public void Constructor_ShouldThrowException_WhenThemeIdIsEmpty()
    {
        // Act
        var action = () => new GameTheme(Guid.NewGuid(), Guid.Empty, Guid.NewGuid());

        // Assert
        action.ShouldThrow<ArgumentException>();
    }
    
    [Fact]
    public void Constructor_ShouldThrowException_WhenExternalGameRecordIdIsEmpty()
    {
        // Act
        var action = () => new GameTheme(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }
}