using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public sealed class CollectionTests
{
    [Fact]
    public void Constructor_ShouldCreateCollection()
    {
        // Arrange
        const string name = "The Legend of Zelda";

        // Act
        var collection = new Collection(name);

        // Assert
        collection.Id.ShouldNotBe(Guid.Empty);
        collection.Name.ShouldBe(name);
        collection.NormalizedName.ShouldBe("THE LEGEND OF ZELDA");
    }

    [Fact]
    public void Constructor_ShouldTrimName()
    {
        // Arrange & Act
        var collection = new Collection("  The Legend of Zelda  ");

        // Assert
        collection.Name.ShouldBe("The Legend of Zelda");
        collection.NormalizedName.ShouldBe("THE LEGEND OF ZELDA");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowException_WhenNameIsInvalid(string? name)
    {
        // Arrange & Act
        var action = () => new Collection(name!);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldAllowNameAtMaximumLength()
    {
        // Arrange
        var name = new string('A', Collection.MaxNameLength);

        // Act
        var collection = new Collection(name);

        // Assert
        collection.Name.Length.ShouldBe(Collection.MaxNameLength);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenNameExceedsMaximumLength()
    {
        // Arrange
        var name = new string('A', Collection.MaxNameLength + 1);

        // Act
        var action = () => new Collection(name);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }
}