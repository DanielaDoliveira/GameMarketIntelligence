using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public sealed class GameThemeTests
{
    [Fact]
    public void Constructor_ShouldCreateGameTheme_WhenValuesAreValid()
    {
        // Arrange
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var themeId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, gameId);

        var externalThemeRecord = CreateLinkedExternalThemeRecord(dataSourceId, themeId);

        // Act
        var gameTheme = new GameTheme(gameId, themeId, externalGameRecord, externalThemeRecord);

        // Assert
        gameTheme.GameId.ShouldBe(gameId);
        gameTheme.ThemeId.ShouldBe(themeId);
        gameTheme.ExternalGameRecordId.ShouldBe(externalGameRecord.Id);
        gameTheme.ExternalThemeRecordId.ShouldBe(externalThemeRecord.Id);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenGameIdIsEmpty()
    {
        var dataSourceId = Guid.NewGuid();
        var themeId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, Guid.NewGuid());

        var externalThemeRecord = CreateLinkedExternalThemeRecord(dataSourceId, themeId);

        var action = () => new GameTheme(Guid.Empty, themeId, externalGameRecord, externalThemeRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("gameId");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenThemeIdIsEmpty()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, gameId);

        var externalThemeRecord = CreateLinkedExternalThemeRecord(dataSourceId, Guid.NewGuid());

        var action = () => new GameTheme(gameId, Guid.Empty, externalGameRecord, externalThemeRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("themeId");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameRecordIsNull()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var themeId = Guid.NewGuid();

        var externalThemeRecord = CreateLinkedExternalThemeRecord(dataSourceId, themeId);

        var action = () => new GameTheme(gameId, themeId, null!, externalThemeRecord);

        action.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("externalGameRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalThemeRecordIsNull()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var themeId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, gameId);

        var action = () => new GameTheme(gameId, themeId, externalGameRecord, null!);

        action.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("externalThemeRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameRecordIsNotLinked()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var themeId = Guid.NewGuid();

        var externalGameRecord = new ExternalGameRecord(dataSourceId, "144542", DateTimeOffset.UtcNow);

        var externalThemeRecord = CreateLinkedExternalThemeRecord(dataSourceId, themeId);

        var action = () => new GameTheme(gameId, themeId, externalGameRecord, externalThemeRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalGameRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameRecordIsLinkedToDifferentGame()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var differentGameId = Guid.NewGuid();
        var themeId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, differentGameId);

        var externalThemeRecord = CreateLinkedExternalThemeRecord(dataSourceId, themeId);

        var action = () => new GameTheme(gameId, themeId, externalGameRecord, externalThemeRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalGameRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalThemeRecordIsNotLinked()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var themeId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, gameId);

        var externalThemeRecord = new ExternalThemeRecord(dataSourceId, "17", DateTimeOffset.UtcNow);

        var action = () => new GameTheme(gameId, themeId, externalGameRecord, externalThemeRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalThemeRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalThemeRecordIsLinkedToDifferentTheme()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var themeId = Guid.NewGuid();
        var differentThemeId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, gameId);

        var externalThemeRecord = CreateLinkedExternalThemeRecord(dataSourceId, differentThemeId);

        var action = () => new GameTheme(gameId, themeId, externalGameRecord, externalThemeRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalThemeRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalRecordsBelongToDifferentDataSources()
    {
        var gameDataSourceId = Guid.NewGuid();
        var themeDataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var themeId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(gameDataSourceId, gameId);

        var externalThemeRecord = CreateLinkedExternalThemeRecord(themeDataSourceId, themeId);

        var action = () => new GameTheme(gameId, themeId, externalGameRecord, externalThemeRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalThemeRecord");
    }

    private static ExternalGameRecord CreateLinkedExternalGameRecord(Guid dataSourceId, Guid gameId)
    {
        var externalGameRecord = new ExternalGameRecord(dataSourceId, "144542", DateTimeOffset.UtcNow);

        externalGameRecord.LinkToGame(gameId);

        return externalGameRecord;
    }

    private static ExternalThemeRecord CreateLinkedExternalThemeRecord(
        Guid dataSourceId,
        Guid themeId)
    {
        var externalThemeRecord = new ExternalThemeRecord(dataSourceId, "17", DateTimeOffset.UtcNow);

        externalThemeRecord.LinkToTheme(themeId);

        return externalThemeRecord;
    }
}