using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public sealed class DataSourceTests
{
    [Fact]
    public void Constructor_ShouldCreateDataSource_WhenDataIsValid()
    {
        // Arrange
        var reliability = new SourceReliability(
            ReliabilityLevel.PublicDirect,
            "Dados obtidos diretamente de uma API pública.",
            "Os dados não representam vendas.");

        const string code = "steam";
        const string name = "Steam Web API";
        const string url = "https://partner.steamgames.com/";
        const string licenseNotes = "Uso sujeito aos termos da Steam.";

        // Act
        var source = new DataSource(
            code,
            name,
            url,
            reliability,
            attributionRequired: false,
            licenseNotes: licenseNotes);

        // Assert
        source.Id.ShouldNotBe(Guid.Empty);
        source.Code.ShouldBe(code);
        source.Name.ShouldBe(name);
        source.Url.ShouldBe(url);
        source.Reliability.ShouldBeSameAs(reliability);
        source.AttributionRequired.ShouldBeFalse();
        source.LicenseNotes.ShouldBe(licenseNotes);
    }

    [Fact]
    public void Constructor_ShouldNormalizeCode_WhenCodeIsValid()
    {
        // Arrange
        var reliability = CreateReliability();

        // Act
        var source = new DataSource(
            " IGDB ",
            "IGDB",
            "https://www.igdb.com/",
            reliability,
            attributionRequired: true);

        // Assert
        source.Code.ShouldBe("igdb");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowArgumentException_WhenCodeIsMissing(
        string invalidCode)
    {
        // Arrange
        var reliability = CreateReliability();

        // Act
        var exception = Should.Throw<ArgumentException>(() =>
        {
            _ = new DataSource(
                invalidCode,
                "IGDB",
                "https://www.igdb.com/",
                reliability,
                attributionRequired: true);
        });

        // Assert
        exception.ParamName.ShouldBe("code");
    }

    [Theory]
    [InlineData("Steam Web API")]
    [InlineData("steam_api")]
    [InlineData("stéam")]
    [InlineData("-steam")]
    [InlineData("steam-")]
    [InlineData("steam--store")]
    public void Constructor_ShouldThrowArgumentException_WhenCodeFormatIsInvalid(
        string invalidCode)
    {
        // Arrange
        var reliability = CreateReliability();

        // Act
        var exception = Should.Throw<ArgumentException>(() =>
        {
            _ = new DataSource(
                invalidCode,
                "Steam",
                "https://store.steampowered.com/",
                reliability,
                attributionRequired: true);
        });

        // Assert
        exception.ParamName.ShouldBe("code");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowArgumentException_WhenNameIsInvalid(
        string invalidName)
    {
        // Arrange
        var reliability = CreateReliability();

        // Act
        var exception = Should.Throw<ArgumentException>(() =>
        {
            _ = new DataSource(
                "steam",
                invalidName,
                "https://partner.steamgames.com/",
                reliability,
                attributionRequired: false);
        });

        // Assert
        exception.ParamName.ShouldBe("name");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("steam")]
    [InlineData("/api/steam")]
    public void Constructor_ShouldThrowArgumentException_WhenUrlIsInvalid(
        string invalidUrl)
    {
        // Arrange
        var reliability = CreateReliability();

        // Act
        var exception = Should.Throw<ArgumentException>(() =>
        {
            _ = new DataSource(
                "steam",
                "Steam Web API",
                invalidUrl,
                reliability,
                attributionRequired: false);
        });

        // Assert
        exception.ParamName.ShouldBe("url");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenReliabilityIsNull()
    {
        // Act
        var exception = Should.Throw<ArgumentNullException>(() =>
        {
            _ = new DataSource(
                "steam",
                "Steam Web API",
                "https://partner.steamgames.com/",
                reliability: null!,
                attributionRequired: false);
        });

        // Assert
        exception.ParamName.ShouldBe("reliability");
    }

    private static SourceReliability CreateReliability()
    {
        return new SourceReliability(
            ReliabilityLevel.PublicDirect,
            "Dados obtidos diretamente de uma API pública.");
    }
}