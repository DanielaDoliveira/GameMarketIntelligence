using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public class GameKeywordTests
{
    [Fact]
    public void Constructor_ShouldInitializeGameKeyword()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var keywordId = Guid.NewGuid();
        var externalGameRecordId = Guid.NewGuid();

        // Act
        var gameKeyword = new GameKeyword(gameId, keywordId, externalGameRecordId);

        // Assert
        gameKeyword.GameId.ShouldBe(gameId);
        gameKeyword.KeywordId.ShouldBe(keywordId);
        gameKeyword.ExternalGameRecordId.ShouldBe(externalGameRecordId);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenGameIdIsEmpty()
    {
        // Act
        var action = () => new GameKeyword(Guid.Empty, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenKeywordIdIsEmpty()
    {
        // Act
        var action = () => new GameKeyword(Guid.NewGuid(), Guid.Empty, Guid.NewGuid());

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenExternalGameRecordIdIsEmpty()
    {
        // Act
        var action = () => new GameKeyword(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }
}