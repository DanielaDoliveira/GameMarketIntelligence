using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public class GameTests
{
    [Fact]
    public void Constructor_ShouldGenerateId()
    {
        var game = new Game("Hades");

        game.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_ShouldStoreName()
    {
        const string name = "Hades";

        var game = new Game(name);

        game.Name.ShouldBe(name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowException_WhenNameIsInvalid(string? name)
    {
        var action = () => new Game(name!);

        action.ShouldThrow<ArgumentException>();
    }
    [Fact]
    public void Constructor_ShouldTrimName()
    {
        var game = new Game("  Hades  ");

        game.Name.ShouldBe("Hades");
    }
    [Fact]
    public void Constructor_ShouldTrimDescription()
    {
        var game = new Game(
            name: "Hades",
            description: "  A roguelike action game.  ");

        game.Description.ShouldBe("A roguelike action game.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldNormalizeInvalidDescriptionToNull(
    string? description)
    {
        var game = new Game(
            name: "Hades",
            description: description);

        game.Description.ShouldBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldNormalizeInvalidOptionalImageUrlToNull(
    string? imageUrl)
    {
        var game = new Game(
            name: "Hades",
            imageUrl: imageUrl);

        game.ImageUrl.ShouldBeNull();
    }

    [Theory]
    [InlineData("https://example.com/hades.png")]
    [InlineData("http://example.com/hades.png")]
    public void Constructor_ShouldStoreValidAbsoluteImageUrl(
    string imageUrl)
    {
        var game = new Game(
            name: "Hades",
            imageUrl: imageUrl);

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
        var action = () => new Game(
            name: "Hades",
            imageUrl: imageUrl);

        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldStoreReleaseDate()
    {
        var releaseDate = new DateOnly(2020, 9, 17);

        var game = new Game(
            name: "Hades",
            releaseDate: releaseDate);

        game.ReleaseDate.ShouldBe(releaseDate);
    }
    [Fact]
    public void Constructor_ShouldAllowNullReleaseDate()
    {
        var game = new Game("Hades");

        game.ReleaseDate.ShouldBeNull();
    }

    [Fact]
    public void AddGenre_ShouldAssociateGenreWithGame()
    {
        var game = new Game("Hades");
        var genre = new Genre("Action");

        game.AddGenre(genre);

        game.Genres.ShouldContain(genre);
    }
    [Fact]
    public void AddGenre_ShouldThrowException_WhenGenreIsNull()
    {
        var game = new Game("Hades");

        var action = () => game.AddGenre(null!);

        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void AddGenre_ShouldNotAddTheSameGenreTwice()
    {
        var game = new Game("Hades");
        var genre = new Genre("Action");

        game.AddGenre(genre);
        game.AddGenre(genre);

        game.Genres.Count.ShouldBe(1);
    }
    [Fact]
    public void AddPlatform_ShouldAssociatePlatformWithGame()
    {
        var game = new Game("Hades");
        var platform = new Platform("PlayStation 5");

        game.AddPlatform(platform);

        game.Platforms.ShouldContain(platform);
    }

    [Fact]
    public void AddPlatform_ShouldThrowException_WhenPlatformIsNull()
    {
        var game = new Game("Hades");

        var action = () => game.AddPlatform(null!);

        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void AddPlatform_ShouldNotAddTheSamePlatformTwice()
    {
        var game = new Game("Hades");
        var platform = new Platform("PlayStation 5");

        game.AddPlatform(platform);
        game.AddPlatform(platform);

        game.Platforms.Count.ShouldBe(1);
    }
}