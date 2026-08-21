using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public sealed class GamePlayerPerspectiveTests
{
    [Fact]
    public void Constructor_ShouldCreateGamePlayerPerspective_WhenValuesAreValid()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var playerPerspectiveId = Guid.NewGuid();
        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, gameId);
        var externalPlayerPerspectiveRecord = CreateLinkedExternalPlayerPerspectiveRecord(dataSourceId, playerPerspectiveId);

        var association = new GamePlayerPerspective(gameId, playerPerspectiveId, externalGameRecord, externalPlayerPerspectiveRecord);

        association.GameId.ShouldBe(gameId);
        association.PlayerPerspectiveId.ShouldBe(playerPerspectiveId);
        association.ExternalGameRecordId.ShouldBe(externalGameRecord.Id);
        association.ExternalPlayerPerspectiveRecordId.ShouldBe(externalPlayerPerspectiveRecord.Id);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenGameIdIsEmpty()
    {
        var dataSourceId = Guid.NewGuid(); var playerPerspectiveId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, Guid.NewGuid());

        var externalPlayerPerspectiveRecord = CreateLinkedExternalPlayerPerspectiveRecord(dataSourceId, playerPerspectiveId);

        var action = () => new GamePlayerPerspective(Guid.Empty, playerPerspectiveId, externalGameRecord, externalPlayerPerspectiveRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("gameId");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenPlayerPerspectiveIdIsEmpty()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, gameId);

        var externalPlayerPerspectiveRecord = CreateLinkedExternalPlayerPerspectiveRecord(dataSourceId, Guid.NewGuid());

        var action = () => new GamePlayerPerspective(gameId, Guid.Empty, externalGameRecord, externalPlayerPerspectiveRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("playerPerspectiveId");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameRecordIsNull()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var playerPerspectiveId = Guid.NewGuid();

        var externalPlayerPerspectiveRecord = CreateLinkedExternalPlayerPerspectiveRecord(dataSourceId, playerPerspectiveId);

        var action = () => new GamePlayerPerspective(gameId, playerPerspectiveId, null!, externalPlayerPerspectiveRecord);

        action.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("externalGameRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalPlayerPerspectiveRecordIsNull()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var playerPerspectiveId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, gameId);

        var action = () => new GamePlayerPerspective(gameId, playerPerspectiveId, externalGameRecord, null!);

        action.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("externalPlayerPerspectiveRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameRecordIsNotLinked()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var playerPerspectiveId = Guid.NewGuid();

        var externalGameRecord = new ExternalGameRecord(dataSourceId, "144542", DateTimeOffset.UtcNow);

        var externalPlayerPerspectiveRecord = CreateLinkedExternalPlayerPerspectiveRecord(dataSourceId, playerPerspectiveId);

        var action = () => new GamePlayerPerspective(gameId, playerPerspectiveId, externalGameRecord, externalPlayerPerspectiveRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalGameRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameRecordIsLinkedToDifferentGame()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var differentGameId = Guid.NewGuid();
        var playerPerspectiveId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, differentGameId);

        var externalPlayerPerspectiveRecord = CreateLinkedExternalPlayerPerspectiveRecord(dataSourceId, playerPerspectiveId);

        var action = () => new GamePlayerPerspective(gameId, playerPerspectiveId, externalGameRecord, externalPlayerPerspectiveRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalGameRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalPlayerPerspectiveRecordIsNotLinked()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var playerPerspectiveId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, gameId);

        var externalPlayerPerspectiveRecord =
            new ExternalPlayerPerspectiveRecord(dataSourceId, "1", DateTimeOffset.UtcNow);

        var action = () => new GamePlayerPerspective(gameId, playerPerspectiveId, externalGameRecord, externalPlayerPerspectiveRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalPlayerPerspectiveRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalPlayerPerspectiveRecordIsLinkedToDifferentPerspective()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var playerPerspectiveId = Guid.NewGuid();
        var differentPlayerPerspectiveId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, gameId);

        var externalPlayerPerspectiveRecord = CreateLinkedExternalPlayerPerspectiveRecord(dataSourceId, differentPlayerPerspectiveId);

        var action = () => new GamePlayerPerspective(gameId, playerPerspectiveId, externalGameRecord, externalPlayerPerspectiveRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalPlayerPerspectiveRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalRecordsBelongToDifferentDataSources()
    {
        var gameDataSourceId = Guid.NewGuid();
        var perspectiveDataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var playerPerspectiveId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(gameDataSourceId, gameId);

        var externalPlayerPerspectiveRecord =
            CreateLinkedExternalPlayerPerspectiveRecord(perspectiveDataSourceId, playerPerspectiveId);

        var action = () => new GamePlayerPerspective(gameId, playerPerspectiveId, externalGameRecord, externalPlayerPerspectiveRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalPlayerPerspectiveRecord");
    }

    private static ExternalGameRecord CreateLinkedExternalGameRecord(Guid dataSourceId, Guid gameId)
    {
        var externalGameRecord = new ExternalGameRecord(dataSourceId, "144542", DateTimeOffset.UtcNow);

        externalGameRecord.LinkToGame(gameId);

        return externalGameRecord;
    }

    private static ExternalPlayerPerspectiveRecord
        CreateLinkedExternalPlayerPerspectiveRecord(Guid dataSourceId, Guid playerPerspectiveId)
    {
        var externalPlayerPerspectiveRecord = new ExternalPlayerPerspectiveRecord(dataSourceId, "1", DateTimeOffset.UtcNow);

        externalPlayerPerspectiveRecord.LinkToPlayerPerspective(playerPerspectiveId);

        return externalPlayerPerspectiveRecord;
    }
}