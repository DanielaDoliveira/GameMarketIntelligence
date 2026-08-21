using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public sealed class ExternalPlayerPerspectiveRecordTests
{
    [Fact]
    public void Constructor_ShouldCreateUnlinkedRecord()
    {
        var dataSourceId = Guid.NewGuid();
        var observedAt = DateTimeOffset.UtcNow;

        var record = new ExternalPlayerPerspectiveRecord(dataSourceId, " 1 ", observedAt);

        record.Id.ShouldNotBe(Guid.Empty);
        record.DataSourceId.ShouldBe(dataSourceId);
        record.ExternalId.ShouldBe("1");
        record.PlayerPerspectiveId.ShouldBeNull();
        record.FirstSeenAt.ShouldBe(observedAt.ToUniversalTime());
        record.LastSeenAt.ShouldBe(observedAt.ToUniversalTime());
        record.SourceUpdatedAt.ShouldBeNull();
    }

    [Fact]
    public void LinkToPlayerPerspective_ShouldLinkRecord_WhenRecordIsUnlinked()
    {
        var record = CreateRecord();
        var playerPerspectiveId = Guid.NewGuid();

        record.LinkToPlayerPerspective(playerPerspectiveId);

        record.PlayerPerspectiveId.ShouldBe(playerPerspectiveId);
    }

    [Fact]
    public void LinkToPlayerPerspective_ShouldBeIdempotent_WhenAlreadyLinkedToSamePerspective()
    {
        var record = CreateRecord();
        var playerPerspectiveId = Guid.NewGuid();

        record.LinkToPlayerPerspective(playerPerspectiveId);

        record.LinkToPlayerPerspective(playerPerspectiveId);

        record.PlayerPerspectiveId.ShouldBe(playerPerspectiveId);
    }

    [Fact]
    public void LinkToPlayerPerspective_ShouldThrow_WhenAlreadyLinkedToDifferentPerspective()
    {
        var record = CreateRecord();
        var firstPlayerPerspectiveId = Guid.NewGuid();
        var secondPlayerPerspectiveId = Guid.NewGuid();

        record.LinkToPlayerPerspective(firstPlayerPerspectiveId);

        var action = () => record.LinkToPlayerPerspective(secondPlayerPerspectiveId);
        
        action.ShouldThrow<InvalidOperationException>();
        record.PlayerPerspectiveId.ShouldBe(firstPlayerPerspectiveId);
    }

    [Fact]
    public void LinkToPlayerPerspective_ShouldThrow_WhenPlayerPerspectiveIdIsEmpty()
    {
        var record = CreateRecord();

        var action = () => record.LinkToPlayerPerspective(Guid.Empty);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("playerPerspectiveId");
    }

    [Fact]
    public void Unlink_ShouldKeepRecordUnlinked_WhenAlreadyUnlinked()
    {
        var record = CreateRecord();

        record.Unlink();

        record.PlayerPerspectiveId.ShouldBeNull();
    }

    [Fact]
    public void Unlink_ShouldThrow_WhenRecordIsLinked()
    {
        var record = CreateRecord();
        var playerPerspectiveId = Guid.NewGuid();

        record.LinkToPlayerPerspective(playerPerspectiveId);

        var action = () => record.Unlink();

        action.ShouldThrow<InvalidOperationException>();
        record.PlayerPerspectiveId.ShouldBe(playerPerspectiveId);
    }

    [Fact]
    public void MarkSeen_ShouldUpdateLastSeenAt()
    {
        var observedAt = new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

        var record = new ExternalPlayerPerspectiveRecord(Guid.NewGuid(), "1", observedAt);

        var nextObservedAt = observedAt.AddDays(1);

        record.MarkSeen(nextObservedAt);

        record.LastSeenAt.ShouldBe(nextObservedAt);
    }

    [Fact]
    public void MarkSeen_ShouldUpdateSourceUpdatedAt_WhenNewer()
    {
        var observedAt = new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

        var sourceUpdatedAt = observedAt.AddHours(-2);

        var record = new ExternalPlayerPerspectiveRecord(Guid.NewGuid(), "1", observedAt, sourceUpdatedAt);

        var newerSourceUpdatedAt = observedAt.AddHours(1);

        record.MarkSeen(observedAt.AddDays(1), newerSourceUpdatedAt);

        record.SourceUpdatedAt.ShouldBe(newerSourceUpdatedAt);
    }

    [Fact]
    public void MarkSeen_ShouldKeepSourceUpdatedAt_WhenIncomingValueIsOlder()
    {
        var observedAt = new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

        var sourceUpdatedAt = observedAt.AddHours(1);

        var record = new ExternalPlayerPerspectiveRecord(Guid.NewGuid(), "1", observedAt, sourceUpdatedAt);

        record.MarkSeen(observedAt.AddDays(1), observedAt);

        record.SourceUpdatedAt.ShouldBe(sourceUpdatedAt);
    }

    [Fact]
    public void MarkSeen_ShouldThrow_WhenObservedAtIsOlderThanLastSeenAt()
    {
        var observedAt = new DateTimeOffset(
            2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

        var record = new ExternalPlayerPerspectiveRecord(
            Guid.NewGuid(),
            "1",
            observedAt);

        var action = () => record.MarkSeen(observedAt.AddTicks(-1));

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("observedAt");
    }

    private static ExternalPlayerPerspectiveRecord CreateRecord()
    {
        return new ExternalPlayerPerspectiveRecord(
            Guid.NewGuid(),
            "1",
            DateTimeOffset.UtcNow);
    }
}