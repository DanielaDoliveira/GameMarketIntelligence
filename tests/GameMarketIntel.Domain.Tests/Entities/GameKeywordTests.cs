using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public sealed class GameKeywordTests
{
    [Fact]
    public void Constructor_ShouldCreateGameKeyword_WhenValuesAreValid()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var keywordId = Guid.NewGuid();
        var externalGameRecord = CreateLinkedExternalGameRecord(gameId);

        // Act
        var association = new GameKeyword(gameId, keywordId, externalGameRecord);

        // Assert
        association.GameId.ShouldBe(gameId);
        association.KeywordId.ShouldBe(keywordId);
        association.ExternalGameRecordId.ShouldBe(externalGameRecord.Id);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenGameIdIsEmpty()
    {
        // Arrange
        var keywordId = Guid.NewGuid();
        var externalGameRecord = CreateLinkedExternalGameRecord(Guid.NewGuid());

        // Act
        var action = () => new GameKeyword(Guid.Empty, keywordId, externalGameRecord);

        // Assert
        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("gameId");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenKeywordIdIsEmpty()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var externalGameRecord = CreateLinkedExternalGameRecord(gameId);

        // Act
        var action = () => new GameKeyword(gameId, Guid.Empty, externalGameRecord);

        // Assert
        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("keywordId");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameRecordIsNull()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var keywordId = Guid.NewGuid();

        // Act
        var action = () => new GameKeyword(gameId, keywordId, null!);

        // Assert
        action.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("externalGameRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameRecordIsNotLinked()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var keywordId = Guid.NewGuid();

        var externalGameRecord = new ExternalGameRecord(Guid.NewGuid(), "144542", DateTimeOffset.UtcNow);

        // Act
        var action = () => new GameKeyword(gameId, keywordId, externalGameRecord);

        // Assert
        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("externalGameRecord");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExternalGameRecordIsLinkedToDifferentGame()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var differentGameId = Guid.NewGuid();
        var keywordId = Guid.NewGuid();

        var externalGameRecord = CreateLinkedExternalGameRecord(differentGameId);

        // Act
        var action = () => new GameKeyword(gameId, keywordId, externalGameRecord);

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