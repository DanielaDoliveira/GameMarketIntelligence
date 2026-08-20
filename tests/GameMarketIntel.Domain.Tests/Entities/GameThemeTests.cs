using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public sealed class GameThemeTests
{
    [Fact]
    public void Constructor_ShouldCreateGameTheme_WhenValuesAreValid()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var themeId = Guid.NewGuid();
        var externalGameRecord = CreateLinkedExternalGameRecord(gameId);

        // Act
        var gameTheme = new GameTheme(gameId, themeId, externalGameRecord);

        // Assert
        gameTheme.GameId.ShouldBe(gameId);
        gameTheme.ThemeId.ShouldBe(themeId);
        gameTheme.ExternalGameRecordId.ShouldBe(externalGameRecord.Id);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenGameIdIsEmpty()
    {
        // Arrange
        var themeId = Guid.NewGuid(); var externalGameRecord = CreateLinkedExternalGameRecord(Guid.NewGuid());

        // Act
        var action = () => new GameTheme(Guid.Empty, themeId, externalGameRecord);

        // Assert
        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("gameId");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenThemeIdIsEmpty()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var externalGameRecord = CreateLinkedExternalGameRecord(gameId);

        // Act
        var action = () => new GameTheme(gameId, Guid.Empty, externalGameRecord);

        // Assert
        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("themeId");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameRecordIsNull()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var themeId = Guid.NewGuid();

        // Act
        var action = () => new GameTheme(gameId, themeId, null!);

        // Assert
        action.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("externalGameRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameRecordIsNotLinked()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var themeId = Guid.NewGuid();

        var externalGameRecord = new ExternalGameRecord(Guid.NewGuid(), "144542", DateTimeOffset.UtcNow);

        // Act
        var action = () => new GameTheme(gameId, themeId, externalGameRecord);

        // Assert
        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalGameRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameRecordIsLinkedToDifferentGame()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var differentGameId = Guid.NewGuid();
        var themeId = Guid.NewGuid();

        var externalGameRecord =
            CreateLinkedExternalGameRecord(differentGameId);

        // Act
        var action = () => new GameTheme(gameId, themeId, externalGameRecord);

        // Assert
        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalGameRecord");
    }

    private static ExternalGameRecord CreateLinkedExternalGameRecord(Guid gameId)
    {
        var externalGameRecord = new ExternalGameRecord(Guid.NewGuid(), "144542", DateTimeOffset.UtcNow);

        externalGameRecord.LinkToGame(gameId);

        return externalGameRecord;
    }
}