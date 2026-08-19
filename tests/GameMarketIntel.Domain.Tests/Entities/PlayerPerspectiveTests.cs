using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public class PlayerPerspectiveTests
{
    [Fact]
    public void Constructor_ShouldGenerateId()
    {
        //Act
        var playerPerspective = new PlayerPerspective("First person");
        //Assert
        playerPerspective.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_ShouldStoreTrimmedName()
    {
        //Act
        var playerPerspective = new PlayerPerspective("  Third person  ");
        //Assert
        playerPerspective.Name.ShouldBe("Third person");
    }

    [Fact]
    public void Constructor_ShouldCreateNormalizedName()
    {
        //Act
        var playerPerspective = new PlayerPerspective("fIrSt PeRsOn");
        //Assert
        playerPerspective.NormalizedName.ShouldBe("FIRST PERSON");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_ShouldThrowException_WhenNameIsInvalid(string? name)
    {
        //Act
        var action = () => new PlayerPerspective(name!);
        //Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldAcceptNameAtMaximumLength()
    {
        //Arrange
        var name = new string('A', PlayerPerspective.MaxNameLength);

        //Act
        var playerPerspective = new PlayerPerspective(name);

        //Assert
        playerPerspective.Name.Length.ShouldBe(PlayerPerspective.MaxNameLength);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenNameExceedsMaximumLength()
    {
        //Arrange
        var name = new string('A', PlayerPerspective.MaxNameLength + 1);

        //Act
        var action = () => new PlayerPerspective(name);
        //Assert
        action.ShouldThrow<ArgumentException>();
    }
}