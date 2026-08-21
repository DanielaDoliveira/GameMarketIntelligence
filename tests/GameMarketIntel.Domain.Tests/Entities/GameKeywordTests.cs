using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public sealed class GameKeywordTests
{
    [Fact]
    public void Constructor_ShouldCreateGameKeyword_WhenValuesAreValid()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var keywordId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, gameId);

        var externalKeywordRecord = CreateLinkedExternalKeywordRecord(dataSourceId, keywordId);

        var association = new GameKeyword(gameId, keywordId, externalGameRecord, externalKeywordRecord);

        association.GameId.ShouldBe(gameId);
        association.KeywordId.ShouldBe(keywordId);
        association.ExternalGameRecordId.ShouldBe(externalGameRecord.Id);
        association.ExternalKeywordRecordId.ShouldBe(externalKeywordRecord.Id);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenGameIdIsEmpty()
    {
        var dataSourceId = Guid.NewGuid();
        var keywordId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, Guid.NewGuid());

        var externalKeywordRecord = CreateLinkedExternalKeywordRecord(dataSourceId, keywordId);

        var action = () => new GameKeyword(Guid.Empty, keywordId, externalGameRecord, externalKeywordRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("gameId");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenKeywordIdIsEmpty()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, gameId);

        var externalKeywordRecord = CreateLinkedExternalKeywordRecord(dataSourceId, Guid.NewGuid());

        var action = () => new GameKeyword(gameId, Guid.Empty, externalGameRecord, externalKeywordRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("keywordId");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameRecordIsNull()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var keywordId = Guid.NewGuid();

        var externalKeywordRecord = CreateLinkedExternalKeywordRecord(dataSourceId, keywordId);

        var action = () => new GameKeyword(gameId, keywordId, null!, externalKeywordRecord);

        action.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("externalGameRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalKeywordRecordIsNull()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var keywordId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, gameId);

        var action = () => new GameKeyword(gameId, keywordId, externalGameRecord, null!);

        action.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("externalKeywordRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameRecordIsNotLinked()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var keywordId = Guid.NewGuid();

        var externalGameRecord = new ExternalGameRecord(dataSourceId, "144542", DateTimeOffset.UtcNow);

        var externalKeywordRecord = CreateLinkedExternalKeywordRecord(dataSourceId, keywordId);

        var action = () => new GameKeyword(gameId, keywordId, externalGameRecord, externalKeywordRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalGameRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameRecordIsLinkedToDifferentGame()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var differentGameId = Guid.NewGuid();
        var keywordId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, differentGameId);

        var externalKeywordRecord = CreateLinkedExternalKeywordRecord(dataSourceId, keywordId);

        var action = () => new GameKeyword(gameId, keywordId, externalGameRecord, externalKeywordRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalGameRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalKeywordRecordIsNotLinked()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var keywordId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, gameId);

        var externalKeywordRecord = new ExternalKeywordRecord(dataSourceId, "42", DateTimeOffset.UtcNow);

        var action = () => new GameKeyword(gameId, keywordId, externalGameRecord, externalKeywordRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalKeywordRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalKeywordRecordIsLinkedToDifferentKeyword()
    {
        var dataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var keywordId = Guid.NewGuid();
        var differentKeywordId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(dataSourceId, gameId);

        var externalKeywordRecord = CreateLinkedExternalKeywordRecord(dataSourceId, differentKeywordId);

        var action = () => new GameKeyword(gameId, keywordId, externalGameRecord, externalKeywordRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalKeywordRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalRecordsBelongToDifferentDataSources()
    {
        var gameDataSourceId = Guid.NewGuid();
        var keywordDataSourceId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var keywordId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(gameDataSourceId, gameId);

        var externalKeywordRecord = CreateLinkedExternalKeywordRecord(keywordDataSourceId, keywordId);

        var action = () => new GameKeyword(gameId, keywordId, externalGameRecord, externalKeywordRecord);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalKeywordRecord");
    }

    private static ExternalGameRecord CreateLinkedExternalGameRecord(Guid dataSourceId, Guid gameId)
    {
        var externalGameRecord = new ExternalGameRecord(dataSourceId, "144542", DateTimeOffset.UtcNow);

        externalGameRecord.LinkToGame(gameId);

        return externalGameRecord;
    }

    private static ExternalKeywordRecord CreateLinkedExternalKeywordRecord(Guid dataSourceId, Guid keywordId)
    {
        var externalKeywordRecord = new ExternalKeywordRecord(dataSourceId, "42", DateTimeOffset.UtcNow);
        externalKeywordRecord.LinkToKeyword(keywordId);
        return externalKeywordRecord;
    }
}