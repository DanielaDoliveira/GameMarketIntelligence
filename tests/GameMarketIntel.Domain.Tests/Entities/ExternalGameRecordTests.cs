using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public sealed class ExternalGameRecordTests
{
    [Fact]
    public void Constructor_ShouldCreateUnlinkedRecord()
    {
        // Arrange
        var dataSourceId = Guid.NewGuid();
        var observedAt = DateTimeOffset.UtcNow;

        // Act
        var record = new ExternalGameRecord(dataSourceId, " 144542 ", observedAt);

        // Assert
        record.Id.ShouldNotBe(Guid.Empty);
        record.DataSourceId.ShouldBe(dataSourceId);
        record.ExternalId.ShouldBe("144542");
        record.GameId.ShouldBeNull();
        record.Status.ShouldBe(ExternalGameRecordStatus.Unlinked);
        record.FirstSeenAt.ShouldBe(observedAt.ToUniversalTime());
        record.LastSeenAt.ShouldBe(observedAt.ToUniversalTime());
        record.SourceUpdatedAt.ShouldBeNull();
    }

    [Fact]
    public void LinkToGame_ShouldLinkRecord_WhenRecordIsUnlinked()
    {
        // Arrange
        var record = CreateRecord();
        var gameId = Guid.NewGuid();

        // Act
        record.LinkToGame(gameId);

        // Assert
        record.GameId.ShouldBe(gameId);
        record.Status.ShouldBe(ExternalGameRecordStatus.Linked);
    }

    [Fact]
    public void LinkToGame_ShouldBeIdempotent_WhenAlreadyLinkedToSameGame()
    {
        // Arrange
        var record = CreateRecord();
        var gameId = Guid.NewGuid();

        record.LinkToGame(gameId);

        // Act
        record.LinkToGame(gameId);

        // Assert
        record.GameId.ShouldBe(gameId);
        record.Status.ShouldBe(ExternalGameRecordStatus.Linked);
    }

    [Fact]
    public void LinkToGame_ShouldThrow_WhenAlreadyLinkedToDifferentGame()
    {
        // Arrange
        var record = CreateRecord();
        var firstGameId = Guid.NewGuid();
        var secondGameId = Guid.NewGuid();

        record.LinkToGame(firstGameId);

        // Act
        var action = () => record.LinkToGame(secondGameId);

        // Assert
        action.ShouldThrow<InvalidOperationException>();
        record.GameId.ShouldBe(firstGameId);
        record.Status.ShouldBe(ExternalGameRecordStatus.Linked);
    }

    [Fact]
    public void LinkToGame_ShouldThrow_WhenRecordIsRejected()
    {
        // Arrange
        var record = CreateRecord();
        record.Reject();

        // Act
        var action = () => record.LinkToGame(Guid.NewGuid());

        // Assert
        action.ShouldThrow<InvalidOperationException>();
        record.GameId.ShouldBeNull();
        record.Status.ShouldBe(ExternalGameRecordStatus.Rejected);
    }

    [Fact]
    public void LinkToGame_ShouldThrow_WhenGameIdIsEmpty()
    {
        // Arrange
        var record = CreateRecord();

        // Act
        var action = () => record.LinkToGame(Guid.Empty);

        // Assert
        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("gameId");
    }

    [Fact]
    public void Unlink_ShouldKeepRecordUnlinked_WhenAlreadyUnlinked()
    {
        // Arrange
        var record = CreateRecord();

        // Act
        record.Unlink();

        // Assert
        record.GameId.ShouldBeNull();
        record.Status.ShouldBe(ExternalGameRecordStatus.Unlinked);
    }

    [Fact]
    public void Unlink_ShouldThrow_WhenRecordIsLinked()
    {
        // Arrange
        var record = CreateRecord();
        var gameId = Guid.NewGuid();
        record.LinkToGame(gameId);

        // Act
        var action = () => record.Unlink();

        // Assert
        action.ShouldThrow<InvalidOperationException>();
        record.GameId.ShouldBe(gameId);
        record.Status.ShouldBe(ExternalGameRecordStatus.Linked);
    }

    [Fact]
    public void Unlink_ShouldThrow_WhenRecordIsRejected()
    {
        // Arrange
        var record = CreateRecord();
        record.Reject();

        // Act
        var action = () => record.Unlink();

        // Assert
        action.ShouldThrow<InvalidOperationException>();
        record.GameId.ShouldBeNull();
        record.Status.ShouldBe(ExternalGameRecordStatus.Rejected);
    }

    [Fact]
    public void Reject_ShouldRejectRecord_WhenRecordIsUnlinked()
    {
        // Arrange
        var record = CreateRecord();

        // Act
        record.Reject();

        // Assert
        record.GameId.ShouldBeNull();
        record.Status.ShouldBe(ExternalGameRecordStatus.Rejected);
    }

    [Fact]
    public void Reject_ShouldBeIdempotent_WhenAlreadyRejected()
    {
        // Arrange
        var record = CreateRecord();
        record.Reject();

        // Act
        record.Reject();

        // Assert
        record.GameId.ShouldBeNull();
        record.Status.ShouldBe(ExternalGameRecordStatus.Rejected);
    }

    [Fact]
    public void Reject_ShouldThrow_WhenRecordIsLinked()
    {
        // Arrange
        var record = CreateRecord();
        var gameId = Guid.NewGuid();
        record.LinkToGame(gameId);

        // Act
        var action = () => record.Reject();

        // Assert
        action.ShouldThrow<InvalidOperationException>();
        record.GameId.ShouldBe(gameId);
        record.Status.ShouldBe(ExternalGameRecordStatus.Linked);
    }

    [Fact]
    public void MarkSeen_ShouldUpdateLastSeenAt()
    {
        // Arrange
        var observedAt = new DateTimeOffset(
            2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

        var record = new ExternalGameRecord(Guid.NewGuid(), "144542", observedAt);

        var nextObservedAt = observedAt.AddDays(1);

        // Act
        record.MarkSeen(nextObservedAt);

        // Assert
        record.LastSeenAt.ShouldBe(nextObservedAt);
    }

    [Fact]
    public void MarkSeen_ShouldUpdateSourceUpdatedAt_WhenNewer()
    {
        // Arrange
        var observedAt = new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

        var sourceUpdatedAt = observedAt.AddHours(-2);

        var record = new ExternalGameRecord(Guid.NewGuid(), "144542", observedAt, sourceUpdatedAt);

        var newerSourceUpdatedAt = observedAt.AddHours(1);

        // Act
        record.MarkSeen(observedAt.AddDays(1), newerSourceUpdatedAt);

        // Assert
        record.SourceUpdatedAt.ShouldBe(newerSourceUpdatedAt);
    }

    [Fact]
    public void MarkSeen_ShouldKeepSourceUpdatedAt_WhenIncomingValueIsOlder()
    {
        // Arrange
        var observedAt = new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

        var sourceUpdatedAt = observedAt.AddHours(1);

        var record = new ExternalGameRecord(Guid.NewGuid(), "144542", observedAt, sourceUpdatedAt);

        // Act
        record.MarkSeen(observedAt.AddDays(1), observedAt);

        // Assert
        record.SourceUpdatedAt.ShouldBe(sourceUpdatedAt);
    }

    [Fact]
    public void MarkSeen_ShouldThrow_WhenObservedAtIsOlderThanLastSeenAt()
    {
        // Arrange
        var observedAt = new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

        var record = new ExternalGameRecord(Guid.NewGuid(), "144542", observedAt);

        // Act
        var action = () => record.MarkSeen(observedAt.AddTicks(-1));

        // Assert
        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("observedAt");
    }

    private static ExternalGameRecord CreateRecord()
    {
        return new ExternalGameRecord(Guid.NewGuid(), "144542", DateTimeOffset.UtcNow);
    }
}