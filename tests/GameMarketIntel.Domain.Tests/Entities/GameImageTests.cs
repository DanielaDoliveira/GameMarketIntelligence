using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public sealed class GameImageTests
{
    [Fact]
    public void Constructor_ShouldCreateGameImage_WhenValuesAreValid()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var externalGameRecord = CreateLinkedExternalGameRecord(gameId);

        // Act
        var image = new GameImage(
            externalGameRecord,
            " 12345 ",
            " co8abc ",
            GameImageType.Cover,
            width: 1080,
            height: 1440,
            sortOrder: 0);

        // Assert
        image.Id.ShouldNotBe(Guid.Empty);
        image.GameId.ShouldBe(gameId);
        image.ExternalGameRecordId.ShouldBe(externalGameRecord.Id);
        image.ExternalId.ShouldBe("12345");
        image.SourceImageId.ShouldBe("co8abc");
        image.Type.ShouldBe(GameImageType.Cover);
        image.Width.ShouldBe(1080);
        image.Height.ShouldBe(1440);
        image.SortOrder.ShouldBe(0);
    }

    [Fact]
    public void Constructor_ShouldAllowNullOptionalMetadata()
    {
        // Arrange
        var externalGameRecord = CreateLinkedExternalGameRecord(Guid.NewGuid());

        // Act
        var image = new GameImage(
            externalGameRecord,
            "12345",
            "co8abc",
            GameImageType.Screenshot);

        // Assert
        image.Width.ShouldBeNull();
        image.Height.ShouldBeNull();
        image.SortOrder.ShouldBeNull();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameRecordIsNull()
    {
        // Arrange & Act
        var action = () => new GameImage(
            null!,
            "12345",
            "co8abc",
            GameImageType.Cover);

        // Assert
        action.ShouldThrow<ArgumentNullException>()
            .ParamName.ShouldBe("externalGameRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameRecordIsNotLinked()
    {
        // Arrange
        var externalGameRecord = new ExternalGameRecord(
            Guid.NewGuid(),
            "144542",
            DateTimeOffset.UtcNow);

        // Act
        var action = () => new GameImage(
            externalGameRecord,
            "12345",
            "co8abc",
            GameImageType.Cover);

        // Assert
        action.ShouldThrow<InvalidOperationException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_ShouldThrow_WhenExternalIdIsInvalid(
        string externalId)
    {
        // Arrange
        var externalGameRecord =
            CreateLinkedExternalGameRecord(Guid.NewGuid());

        // Act
        var action = () => new GameImage(
            externalGameRecord,
            externalId,
            "co8abc",
            GameImageType.Cover);

        // Assert
        action.ShouldThrow<ArgumentException>()
            .ParamName.ShouldBe("externalId");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalIdExceedsMaxLength()
    {
        // Arrange
        var externalGameRecord =
            CreateLinkedExternalGameRecord(Guid.NewGuid());

        var externalId =
            new string('x', GameImage.MaxExternalIdLength + 1);

        // Act
        var action = () => new GameImage(
            externalGameRecord,
            externalId,
            "co8abc",
            GameImageType.Cover);

        // Assert
        action.ShouldThrow<ArgumentException>()
            .ParamName.ShouldBe("externalId");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_ShouldThrow_WhenSourceImageIdIsInvalid(
        string sourceImageId)
    {
        // Arrange
        var externalGameRecord =
            CreateLinkedExternalGameRecord(Guid.NewGuid());

        // Act
        var action = () => new GameImage(
            externalGameRecord,
            "12345",
            sourceImageId,
            GameImageType.Cover);

        // Assert
        action.ShouldThrow<ArgumentException>()
            .ParamName.ShouldBe("sourceImageId");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenSourceImageIdExceedsMaxLength()
    {
        // Arrange
        var externalGameRecord =
            CreateLinkedExternalGameRecord(Guid.NewGuid());

        var sourceImageId =
            new string('x', GameImage.MaxSourceImageIdLength + 1);

        // Act
        var action = () => new GameImage(
            externalGameRecord,
            "12345",
            sourceImageId,
            GameImageType.Cover);

        // Assert
        action.ShouldThrow<ArgumentException>()
            .ParamName.ShouldBe("sourceImageId");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldThrow_WhenWidthIsInvalid(int width)
    {
        // Arrange
        var externalGameRecord =
            CreateLinkedExternalGameRecord(Guid.NewGuid());

        // Act
        var action = () => new GameImage(
            externalGameRecord,
            "12345",
            "co8abc",
            GameImageType.Cover,
            width: width);

        // Assert
        action.ShouldThrow<ArgumentOutOfRangeException>()
            .ParamName.ShouldBe("width");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldThrow_WhenHeightIsInvalid(int height)
    {
        // Arrange
        var externalGameRecord =
            CreateLinkedExternalGameRecord(Guid.NewGuid());

        // Act
        var action = () => new GameImage(
            externalGameRecord,
            "12345",
            "co8abc",
            GameImageType.Cover,
            height: height);

        // Assert
        action.ShouldThrow<ArgumentOutOfRangeException>()
            .ParamName.ShouldBe("height");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenSortOrderIsNegative()
    {
        // Arrange
        var externalGameRecord =
            CreateLinkedExternalGameRecord(Guid.NewGuid());

        // Act
        var action = () => new GameImage(
            externalGameRecord,
            "12345",
            "co8abc",
            GameImageType.Screenshot,
            sortOrder: -1);

        // Assert
        action.ShouldThrow<ArgumentOutOfRangeException>()
            .ParamName.ShouldBe("sortOrder");
    }

    private static ExternalGameRecord CreateLinkedExternalGameRecord(
        Guid gameId)
    {
        var externalGameRecord = new ExternalGameRecord(
            Guid.NewGuid(),
            "144542",
            DateTimeOffset.UtcNow);

        externalGameRecord.LinkToGame(gameId);

        return externalGameRecord;
    }
}