using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public class ThemeTests
{
    [Fact]
    public void Constructor_ShouldGenerateId()
    {
        //Act
        var theme = new Theme("Horror");
        //Assert
        theme.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_ShouldStoreTrimmedName()
    {
        //Act
        var theme = new Theme("  Adventure  ");
        //Assert
        theme.Name.ShouldBe("Adventure");
    }

    [Fact]
    public void Constructor_ShouldCreateNormalizedName()
    {
        //Act
        var theme = new Theme("PLatForM");
        //Assert
        theme.NormalizedName.ShouldBe("PLATFORM");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_ShouldThrowException_WhenNameIsInvalid(string? name)
    {
        //Act
        var action = () => new Theme(name!);
        //Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldAcceptNameAtMaximumLength()
    {
        //Arrange
        var name = new string('A', Theme.MaxNameLength);

        //Act
        var theme = new Theme(name);

        //Assert
        theme.Name.Length.ShouldBe(Theme.MaxNameLength);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenNameExceedsMaximumLength()
    {
        //Arrange
        var name = new string('A', Theme.MaxNameLength + 1);

        //Act
        var action = () => new Theme(name);
        //Assert
        action.ShouldThrow<ArgumentException>();
    }
}