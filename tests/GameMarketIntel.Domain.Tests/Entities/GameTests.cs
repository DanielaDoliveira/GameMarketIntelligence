using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public class GameTests
{
    [Fact]
    public void Constructor_ShouldGenerateId()
    {
        // Arrange & Act
        var game = new Game("Hades");

        // Assert
        game.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_ShouldStoreName()
    {
        // Arrange
        const string name = "Hades";

        // Act
        var game = new Game(name);

        // Assert
        game.Name.ShouldBe(name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowException_WhenNameIsInvalid(string? name)
    {
        // Arrange & Act
        var action = () => new Game(name!);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldTrimName()
    {
        // Arrange & Act
        var game = new Game("  Hades  ");

        // Assert
        game.Name.ShouldBe("Hades");
    }

    [Fact]
    public void Constructor_ShouldTrimDescription()
    {
        // Arrange
        const string description = "  A roguelike action game.  ";

        // Act
        var game = new Game(
            name: "Hades",
            description: description);

        // Assert
        game.Description.ShouldBe("A roguelike action game.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldNormalizeInvalidDescriptionToNull(
        string? description)
    {
        // Arrange & Act
        var game = new Game(
            name: "Hades",
            description: description);

        // Assert
        game.Description.ShouldBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldNormalizeInvalidOptionalImageUrlToNull(
        string? imageUrl)
    {
        // Arrange & Act
        var game = new Game(
            name: "Hades",
            imageUrl: imageUrl);

        // Assert
        game.ImageUrl.ShouldBeNull();
    }

    [Theory]
    [InlineData("https://example.com/hades.png")]
    [InlineData("http://example.com/hades.png")]
    public void Constructor_ShouldStoreValidAbsoluteImageUrl(
        string imageUrl)
    {
        // Arrange & Act
        var game = new Game(
            name: "Hades",
            imageUrl: imageUrl);

        // Assert
        game.ImageUrl.ShouldBe(imageUrl);
    }

    [Theory]
    [InlineData("hades.png")]
    [InlineData("/images/hades.png")]
    [InlineData("ftp://example.com/hades.png")]
    [InlineData("not-a-url")]
    public void Constructor_ShouldThrowException_WhenImageUrlIsInvalid(
        string imageUrl)
    {
        // Arrange & Act
        var action = () => new Game(
            name: "Hades",
            imageUrl: imageUrl);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldStoreFirstReleaseDate()
    {
        // Arrange
        var firstReleaseDate = new DateOnly(2020, 9, 17);

        // Act
        var game = new Game(
            name: "Hades",
            firstReleaseDate: firstReleaseDate);

        // Assert
        game.FirstReleaseDate.ShouldBe(firstReleaseDate);
    }

    [Fact]
    public void Constructor_ShouldAllowNullFirstReleaseDate()
    {
        // Arrange & Act
        var game = new Game("Hades");

        // Assert
        game.FirstReleaseDate.ShouldBeNull();
    }

    [Fact]
    public void AddGenre_ShouldAssociateGenreWithGame()
    {
        // Arrange
        var game = new Game("Hades");
        var genre = new Genre("Action");

        // Act
        game.AddGenre(genre);

        // Assert
        game.Genres.ShouldContain(genre);
    }

    [Fact]
    public void AddGenre_ShouldThrowException_WhenGenreIsNull()
    {
        // Arrange
        var game = new Game("Hades");

        // Act
        var action = () => game.AddGenre(null!);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void AddGenre_ShouldNotAddTheSameGenreTwice()
    {
        // Arrange
        var game = new Game("Hades");
        var genre = new Genre("Action");

        // Act
        game.AddGenre(genre);
        game.AddGenre(genre);

        // Assert
        game.Genres.Count.ShouldBe(1);
    }

    [Fact]
    public void AddPlatform_ShouldAssociatePlatformWithGame()
    {
        // Arrange
        var game = new Game("Hades");
        var platform = new Platform("PlayStation 5");

        // Act
        game.AddPlatform(platform);

        // Assert
        game.Platforms.ShouldContain(platform);
    }

    [Fact]
    public void AddPlatform_ShouldThrowException_WhenPlatformIsNull()
    {
        // Arrange
        var game = new Game("Hades");

        // Act
        var action = () => game.AddPlatform(null!);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void AddPlatform_ShouldNotAddTheSamePlatformTwice()
    {
        // Arrange
        var game = new Game("Hades");
        var platform = new Platform("PlayStation 5");

        // Act
        game.AddPlatform(platform);
        game.AddPlatform(platform);

        // Assert
        game.Platforms.Count.ShouldBe(1);
    }
}