using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public class KeywordTests
{
    [Fact]
    public void Constructor_ShouldGenerateId()
    {
        //Act
        var keyword = new Keyword("Time travel");
        //Assert
        keyword.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_ShouldStoreTrimmedName()
    {
        //Act
        var keyword = new Keyword("  Memory loss  ");
        //Assert
        keyword.Name.ShouldBe("Memory loss");
    }

    [Fact]
    public void Constructor_ShouldCreateNormalizedName()
    {
        //Act
        var keyword = new Keyword("tIme lOoP");
        //Assert
        keyword.NormalizedName.ShouldBe("TIME LOOP");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_ShouldThrowException_WhenNameIsInvalid(string? name)
    {
        //Act
        var action = () => new Keyword(name!);
        //Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldAcceptNameAtMaximumLength()
    {
        //Arrange
        var name = new string('A', Keyword.MaxNameLength);

        //Act
        var keyword = new Keyword(name);

        //Assert
        keyword.Name.Length.ShouldBe(Keyword.MaxNameLength);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenNameExceedsMaximumLength()
    {
        //Arrange
        var name = new string('A', Keyword.MaxNameLength + 1);

        //Act
        var action = () => new Keyword(name);
        //Assert
        action.ShouldThrow<ArgumentException>();
    }
}