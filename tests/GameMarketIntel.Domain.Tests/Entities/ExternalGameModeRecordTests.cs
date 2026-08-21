using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public sealed class ExternalGameModeRecordTests
{
    [Fact]
    public void Constructor_ShouldCreateUnlinkedRecord()
    {
        var dataSourceId = Guid.NewGuid();
        var observedAt = DateTimeOffset.UtcNow;

        var record = new ExternalGameModeRecord(
            dataSourceId,
            " 1 ",
            observedAt);

        record.Id.ShouldNotBe(Guid.Empty);
        record.DataSourceId.ShouldBe(dataSourceId);
        record.ExternalId.ShouldBe("1");
        record.GameModeId.ShouldBeNull();
        record.FirstSeenAt.ShouldBe(observedAt.ToUniversalTime());
        record.LastSeenAt.ShouldBe(observedAt.ToUniversalTime());
        record.SourceUpdatedAt.ShouldBeNull();
    }

    [Fact]
    public void LinkToGameMode_ShouldLinkRecord_WhenRecordIsUnlinked()
    {
        var record = CreateRecord();
        var gameModeId = Guid.NewGuid();

        record.LinkToGameMode(gameModeId);

        record.GameModeId.ShouldBe(gameModeId);
    }

    [Fact]
    public void LinkToGameMode_ShouldBeIdempotent_WhenAlreadyLinkedToSameGameMode()
    {
        var record = CreateRecord();
        var gameModeId = Guid.NewGuid();

        record.LinkToGameMode(gameModeId);

        record.LinkToGameMode(gameModeId);

        record.GameModeId.ShouldBe(gameModeId);
    }

    [Fact]
    public void LinkToGameMode_ShouldThrow_WhenAlreadyLinkedToDifferentGameMode()
    {
        var record = CreateRecord();
        var firstGameModeId = Guid.NewGuid();
        var secondGameModeId = Guid.NewGuid();

        record.LinkToGameMode(firstGameModeId);

        var action = () => record.LinkToGameMode(secondGameModeId);

        action.ShouldThrow<InvalidOperationException>();
        record.GameModeId.ShouldBe(firstGameModeId);
    }

    [Fact]
    public void LinkToGameMode_ShouldThrow_WhenGameModeIdIsEmpty()
    {
        var record = CreateRecord();

        var action = () => record.LinkToGameMode(Guid.Empty);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("gameModeId");
    }

    [Fact]
    public void Unlink_ShouldKeepRecordUnlinked_WhenAlreadyUnlinked()
    {
        var record = CreateRecord();

        record.Unlink();
        
        record.GameModeId.ShouldBeNull();
    }

    [Fact]
    public void Unlink_ShouldThrow_WhenRecordIsLinked()
    {
        var record = CreateRecord();
        var gameModeId = Guid.NewGuid();

        record.LinkToGameMode(gameModeId);

        var action = () => record.Unlink();

        action.ShouldThrow<InvalidOperationException>();
        record.GameModeId.ShouldBe(gameModeId);
    }

    [Fact]
    public void MarkSeen_ShouldUpdateLastSeenAt()
    {
        var observedAt = new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

        var record = new ExternalGameModeRecord(Guid.NewGuid(), "1", observedAt);

        var nextObservedAt = observedAt.AddDays(1);

        record.MarkSeen(nextObservedAt);

        record.LastSeenAt.ShouldBe(nextObservedAt);
    }

    [Fact]
    public void MarkSeen_ShouldUpdateSourceUpdatedAt_WhenNewer()
    {
        var observedAt = new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

        var sourceUpdatedAt = observedAt.AddHours(-2);

        var record = new ExternalGameModeRecord(Guid.NewGuid(), "1", observedAt, sourceUpdatedAt);

        var newerSourceUpdatedAt = observedAt.AddHours(1);

        record.MarkSeen(observedAt.AddDays(1), newerSourceUpdatedAt);

        record.SourceUpdatedAt.ShouldBe(newerSourceUpdatedAt);
    }

    [Fact]
    public void MarkSeen_ShouldKeepSourceUpdatedAt_WhenIncomingValueIsOlder()
    {
        var observedAt = new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

        var sourceUpdatedAt = observedAt.AddHours(1);

        var record = new ExternalGameModeRecord(Guid.NewGuid(), "1", observedAt, sourceUpdatedAt);

        record.MarkSeen(observedAt.AddDays(1), observedAt);

        record.SourceUpdatedAt.ShouldBe(sourceUpdatedAt);
    }

    [Fact]
    public void MarkSeen_ShouldThrow_WhenObservedAtIsOlderThanLastSeenAt()
    {
        var observedAt = new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

        var record = new ExternalGameModeRecord(Guid.NewGuid(), "1", observedAt);

        var action = () => record.MarkSeen(observedAt.AddTicks(-1));

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("observedAt");
    }

    private static ExternalGameModeRecord CreateRecord()
    {
        return new ExternalGameModeRecord(Guid.NewGuid(), "1", DateTimeOffset.UtcNow);
    }
}