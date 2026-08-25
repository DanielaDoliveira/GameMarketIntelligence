using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Infrastructure.ExternalServices.Images;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.ExternalServices.Images;

public sealed class GameImageUrlResolverTests
{
    private readonly GameImageUrlResolver _resolver = new();

    [Fact]
    public void Resolve_ShouldReturnIgdbCoverUrl()
    {
        // Arrange
        const string dataSourceCode = "igdb";
        const string sourceImageId = "co8abc";

        // Act
        var result = _resolver.Resolve(
            dataSourceCode,
            sourceImageId,
            GameImageType.Cover);

        // Assert
        result.ShouldBe("https://images.igdb.com/igdb/image/upload/t_cover_big/co8abc.jpg");
    }

    [Fact]
    public void Resolve_ShouldReturnIgdbScreenshotUrl()
    {
        // Arrange
        const string dataSourceCode = "igdb";
        const string sourceImageId = "sc8abc";

        // Act
        var result = _resolver.Resolve(dataSourceCode, sourceImageId, GameImageType.Screenshot);

        // Assert
        result.ShouldBe("https://images.igdb.com/igdb/image/upload/t_screenshot_big/sc8abc.jpg");
    }

    [Fact]
    public void Resolve_ShouldIgnoreDataSourceCodeCasing()
    {
        // Arrange
        const string dataSourceCode = "IGDB";
        const string sourceImageId = "co8abc";

        // Act
        var result = _resolver.Resolve(dataSourceCode, sourceImageId, GameImageType.Cover);

        // Assert
        result.ShouldBe("https://images.igdb.com/igdb/image/upload/t_cover_big/co8abc.jpg");
    }

    [Fact]
    public void Resolve_ShouldTrimDataSourceCodeAndSourceImageId()
    {
        // Arrange
        const string dataSourceCode = "  igdb  ";
        const string sourceImageId = "  co8abc  ";

        // Act
        var result = _resolver.Resolve(dataSourceCode, sourceImageId, GameImageType.Cover);

        // Assert
        result.ShouldBe(
            "https://images.igdb.com/igdb/image/upload/t_cover_big/co8abc.jpg");
    }

    [Fact]
    public void Resolve_ShouldReturnNull_WhenDataSourceIsUnsupported()
    {
        // Arrange
        const string dataSourceCode = "other-source";
        const string sourceImageId = "image-1";

        // Act
        var result = _resolver.Resolve(dataSourceCode, sourceImageId, GameImageType.Cover);

        // Assert
        result.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_ShouldReturnNull_WhenDataSourceCodeIsInvalid(
        string dataSourceCode)
    {
        // Arrange
        const string sourceImageId = "co8abc";

        // Act
        var result = _resolver.Resolve(dataSourceCode, sourceImageId, GameImageType.Cover);

        // Assert
        result.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_ShouldReturnNull_WhenSourceImageIdIsInvalid(
        string sourceImageId)
    {
        // Arrange
        const string dataSourceCode = "igdb";

        // Act
        var result = _resolver.Resolve(dataSourceCode, sourceImageId, GameImageType.Cover);

        // Assert
        result.ShouldBeNull();
    }
}