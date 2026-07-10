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
                "Os dados não representam vendas."
            );

        const string name = "Steam Web API";
        const string url = "https://partner.steamgames.com/";
        const string licenseNotes = "Uso sujeito aos termos da Steam.";

        // Act
        var source = new DataSource(
            name,
            url,
            reliability,
            attributionRequired: false,
            licenseNotes);

        // Assert
        source.Id.ShouldNotBe(Guid.Empty);

        source.Name.ShouldBe(name);

        source.Url.ShouldBe(url);

        source.Reliability.ShouldBeSameAs(reliability);

        source.AttributionRequired.ShouldBeFalse();

        source.LicenseNotes.ShouldBe(licenseNotes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowArgumentException_WhenNameIsInvalid(
    string invalidName)
    {
        // Arrange
        var reliability = new SourceReliability(
                ReliabilityLevel.PublicDirect,
                "Dados obtidos diretamente de uma API pública.");

        // Act
        var exception = Should.Throw<ArgumentException>(() =>
            new DataSource(
                    invalidName,
                    "https://partner.steamgames.com/",
                    reliability,
                    attributionRequired: false));

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
        var reliability = new SourceReliability(
            ReliabilityLevel.PublicDirect,
            "Dados obtidos diretamente de uma API pública.");

        // Act
        var exception = Should.Throw<ArgumentException>(() =>
            new DataSource(
                "Steam Web API",
                invalidUrl,
                reliability,
                attributionRequired: false));

        // Assert
        exception.ParamName.ShouldBe("url");
    }
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenReliabilityIsNull()
    {
        // Act
        var exception = Should.Throw<ArgumentNullException>(() =>
            new DataSource(
                "Steam Web API",
                "https://partner.steamgames.com/",
                reliability: null!,
                attributionRequired: false));

        // Assert
        exception.ParamName.ShouldBe("reliability");
    }
}