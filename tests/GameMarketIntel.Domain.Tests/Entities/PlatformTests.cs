using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public class PlatformTests
{
    [Fact]
    public void Constructor_ShouldGenerateId()
    {
        var platform = new Platform("PlayStation 5");

        platform.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_ShouldStoreName()
    {
        const string name = "PlayStation 5";

        var platform = new Platform(name);

        platform.Name.ShouldBe(name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowException_WhenNameIsInvalid(string? name)
    {
        var action = () => new Platform(name!);

        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldTrimName()
    {
        var platform = new Platform("  PlayStation 5  ");

        platform.Name.ShouldBe("PlayStation 5");
    }

    [Fact]
    public void Constructor_ShouldTrimOptionalTextFields()
    {
        var platform = new Platform(
            name: "PlayStation 5",
            family: "  PlayStation  ",
            manufacturer: "  Sony  ");

        platform.Family.ShouldBe("PlayStation");
        platform.Manufacturer.ShouldBe("Sony");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldNormalizeInvalidOptionalTextToNull(
    string? optionalText)
    {
        var platform = new Platform(
            name: "PlayStation 5",
            family: optionalText,
            manufacturer: optionalText);

        platform.Family.ShouldBeNull();
        platform.Manufacturer.ShouldBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldNormalizeInvalidOptionalImageUrlToNull(
    string? imageUrl)
    {
        var platform = new Platform(
            name: "PlayStation 5",
            imageUrl: imageUrl);

        platform.ImageUrl.ShouldBeNull();
    }

    [Theory]
    [InlineData("https://example.com/platform.png")]
    [InlineData("http://example.com/platform.png")]
    public void Constructor_ShouldStoreValidAbsoluteImageUrl(
    string imageUrl)
    {
        var platform = new Platform(
            name: "PlayStation 5",
            imageUrl: imageUrl);

        platform.ImageUrl.ShouldBe(imageUrl);
    }

    [Theory]
    [InlineData("platform.png")]
    [InlineData("/images/platform.png")]
    [InlineData("ftp://example.com/platform.png")]
    [InlineData("not-a-url")]
    public void Constructor_ShouldThrowException_WhenImageUrlIsInvalid(
    string imageUrl)
    {
        var action = () => new Platform(
            name: "PlayStation 5",
            imageUrl: imageUrl);

        action.ShouldThrow<ArgumentException>();
    }

}