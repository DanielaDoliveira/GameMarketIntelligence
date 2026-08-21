using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public sealed class GameGameModeTests
{
    [Fact]
    public void Constructor_ShouldCreateGameGameMode_WhenValuesAreValid()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var gameModeId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, gameId);

        var externalGameModeRecord = CreateLinkedExternalGameModeRecord(dataSourceId, gameModeId);

        var association = new GameGameMode(gameId, gameModeId, externalGameRecord, externalGameModeRecord);

        association.GameId.ShouldBe(gameId);
        association.GameModeId.ShouldBe(gameModeId);
        association.ExternalGameRecordId.ShouldBe(externalGameRecord.Id);
        association.ExternalGameModeRecordId.ShouldBe(externalGameModeRecord.Id);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenGameIdIsEmpty()
    {
        var dataSourceId = Guid.NewGuid();
        var gameModeId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, Guid.NewGuid());

        var externalGameModeRecord = CreateLinkedExternalGameModeRecord(dataSourceId, gameModeId);

        var action = () => new GameGameMode(Guid.Empty, gameModeId, externalGameRecord, externalGameModeRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("gameId");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenGameModeIdIsEmpty()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, gameId);

        var externalGameModeRecord = CreateLinkedExternalGameModeRecord(dataSourceId, Guid.NewGuid());

        var action = () => new GameGameMode(gameId, Guid.Empty, externalGameRecord, externalGameModeRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("gameModeId");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameRecordIsNull()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var gameModeId = Guid.NewGuid();

        var externalGameModeRecord = CreateLinkedExternalGameModeRecord(dataSourceId, gameModeId);

        var action = () => new GameGameMode(gameId, gameModeId, null!, externalGameModeRecord);

        action.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("externalGameRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameModeRecordIsNull()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var gameModeId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, gameId);

        var action = () => new GameGameMode(gameId, gameModeId, externalGameRecord,
            null!);

        action.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("externalGameModeRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameRecordIsNotLinked()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var gameModeId = Guid.NewGuid();

        var externalGameRecord = new ExternalGameRecord(dataSourceId, "144542", DateTimeOffset.UtcNow);

        var externalGameModeRecord = CreateLinkedExternalGameModeRecord(dataSourceId, gameModeId);

        var action = () => new GameGameMode(gameId, gameModeId, externalGameRecord, externalGameModeRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalGameRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameRecordIsLinkedToDifferentGame()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var differentGameId = Guid.NewGuid();
        var gameModeId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, differentGameId);

        var externalGameModeRecord = CreateLinkedExternalGameModeRecord(dataSourceId, gameModeId);

        var action = () => new GameGameMode(gameId, gameModeId, externalGameRecord, externalGameModeRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalGameRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameModeRecordIsNotLinked()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var gameModeId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, gameId);

        var externalGameModeRecord = new ExternalGameModeRecord(dataSourceId, "1", DateTimeOffset.UtcNow);

        var action = () => new GameGameMode(gameId, gameModeId, externalGameRecord, externalGameModeRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalGameModeRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameModeRecordIsLinkedToDifferentGameMode()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var gameModeId = Guid.NewGuid();
        var differentGameModeId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, gameId);

        var externalGameModeRecord = CreateLinkedExternalGameModeRecord(dataSourceId, differentGameModeId);

        var action = () => new GameGameMode(gameId, gameModeId, externalGameRecord, externalGameModeRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalGameModeRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalRecordsBelongToDifferentDataSources()
    {
        var gameDataSourceId = Guid.NewGuid();
        var gameModeDataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var gameModeId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(gameDataSourceId, gameId);

        var externalGameModeRecord = CreateLinkedExternalGameModeRecord(gameModeDataSourceId, gameModeId);

        var action = () => new GameGameMode(gameId, gameModeId, externalGameRecord, externalGameModeRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalGameModeRecord");
    }

    private static ExternalGameRecord CreateLinkedExternalGameRecord(Guid dataSourceId, Guid gameId)
    {
        var externalGameRecord = new ExternalGameRecord(dataSourceId, "144542", DateTimeOffset.UtcNow);

        externalGameRecord.LinkToGame(gameId);

        return externalGameRecord;
    }

    private static ExternalGameModeRecord CreateLinkedExternalGameModeRecord(Guid dataSourceId, Guid gameModeId)
    {
        var externalGameModeRecord = new ExternalGameModeRecord(dataSourceId, "1", DateTimeOffset.UtcNow);

        externalGameModeRecord.LinkToGameMode(gameModeId);

        return externalGameModeRecord;
    }
}