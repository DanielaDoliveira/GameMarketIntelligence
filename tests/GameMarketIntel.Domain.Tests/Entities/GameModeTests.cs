using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public class GameModeTests
{
    [Fact]
    public void Constructor_ShouldGenerateId()
    {
        //Act
        
        var gameMode = new GameMode("Single Player");
        
        //Assert
        gameMode.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_ShouldStoreTrimmedName()
    {
        //Act
        
        var gameMode = new GameMode("  Multiplayer  ");
        
        //Assert
        gameMode.Name.ShouldBe("Multiplayer");
    }

    [Fact]
    public void Constructor_ShouldCreateNormalizedName()
    {
        //Act
        
        var gameMode = new GameMode("sInGlE pLaYeR");
        
        //Assert
        
        gameMode.NormalizedName.ShouldBe("SINGLE PLAYER");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_ShouldThrowException_WhenNameIsInvalid(string? name)
    {
        //Act
        
        var action = () => new GameMode(name!);

        //Assert
        
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldAcceptNameAtMaximumLength()
    {
        // Arrange
        
        var name = new string('A', GameMode.MaxNameLength);

        // Act
        
        var gameMode = new GameMode(name);

        // Assert
        
        gameMode.Name.Length.ShouldBe(GameMode.MaxNameLength);
    }


    [Fact]
    public void Constructor_ShouldThrowException_WhenNameExceedsMaximumLength()
    {
        //Arrange
        
        var name = new string('A', GameMode.MaxNameLength + 1);

        //Act
        
        var action = () => new GameMode(name);
        
        //Assert
        
        action.ShouldThrow<ArgumentException>();
    }
}