using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public class GenreTests
{
    [Fact]
    public void Constructor_ShouldGenerateId()
    {
        // Arrange & Act
        var genre = new Genre("Action");

        // Assert
        genre.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_ShouldStoreName()
    {
        // Arrange
        const string name = "Action";

        // Act
        var genre = new Genre(name);

        // Assert
        genre.Name.ShouldBe(name);
    }
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowException_WhenNameIsInvalid(string? name)
    {
        // Act
        var action = () => new Genre(name!);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }
    [Fact]
    public void Constructor_ShouldTrimName()
    {
        // Act
        var genre = new Genre("  Action  ");

        // Assert
        genre.Name.ShouldBe("Action");
    }
}