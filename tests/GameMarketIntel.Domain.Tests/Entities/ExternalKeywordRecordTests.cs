using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public sealed class ExternalKeywordRecordTests
{
    [Fact]
    public void Constructor_ShouldCreateUnlinkedRecord()
    {
        var dataSourceId = Guid.NewGuid();
        var observedAt = DateTimeOffset.UtcNow;

        var record = new ExternalKeywordRecord(dataSourceId, " 42 ", observedAt);

        record.Id.ShouldNotBe(Guid.Empty);
        record.DataSourceId.ShouldBe(dataSourceId);
        record.ExternalId.ShouldBe("42");
        record.KeywordId.ShouldBeNull();
        record.FirstSeenAt.ShouldBe(observedAt.ToUniversalTime());
        record.LastSeenAt.ShouldBe(observedAt.ToUniversalTime());
        record.SourceUpdatedAt.ShouldBeNull();
    }

    [Fact]
    public void LinkToKeyword_ShouldLinkRecord_WhenRecordIsUnlinked()
    {
        var record = CreateRecord();
        var keywordId = Guid.NewGuid();

        record.LinkToKeyword(keywordId);

        record.KeywordId.ShouldBe(keywordId);
    }

    [Fact]
    public void LinkToKeyword_ShouldBeIdempotent_WhenAlreadyLinkedToSameKeyword()
    {
        var record = CreateRecord();
        var keywordId = Guid.NewGuid();

        record.LinkToKeyword(keywordId);

        record.LinkToKeyword(keywordId);

        record.KeywordId.ShouldBe(keywordId);
    }

    [Fact]
    public void LinkToKeyword_ShouldThrow_WhenAlreadyLinkedToDifferentKeyword()
    {
        var record = CreateRecord();
        var firstKeywordId = Guid.NewGuid();
        var secondKeywordId = Guid.NewGuid();

        record.LinkToKeyword(firstKeywordId);

        var action = () => record.LinkToKeyword(secondKeywordId);

        action.ShouldThrow<InvalidOperationException>();
        record.KeywordId.ShouldBe(firstKeywordId);
    }

    [Fact]
    public void LinkToKeyword_ShouldThrow_WhenKeywordIdIsEmpty()
    {
        var record = CreateRecord();

        var action = () => record.LinkToKeyword(Guid.Empty);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("keywordId");
    }

    [Fact]
    public void Unlink_ShouldKeepRecordUnlinked_WhenAlreadyUnlinked()
    {
        var record = CreateRecord();

        record.Unlink();

        record.KeywordId.ShouldBeNull();
    }

    [Fact]
    public void Unlink_ShouldThrow_WhenRecordIsLinked()
    {
        var record = CreateRecord();
        var keywordId = Guid.NewGuid();

        record.LinkToKeyword(keywordId);

        var action = () => record.Unlink();

        action.ShouldThrow<InvalidOperationException>();
        record.KeywordId.ShouldBe(keywordId);
    }

    [Fact]
    public void MarkSeen_ShouldUpdateLastSeenAt()
    {
        var observedAt = new DateTimeOffset(
            2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

        var record = new ExternalKeywordRecord(Guid.NewGuid(), "42", observedAt);

        var nextObservedAt = observedAt.AddDays(1);

        record.MarkSeen(nextObservedAt);

        record.LastSeenAt.ShouldBe(nextObservedAt);
    }

    [Fact]
    public void MarkSeen_ShouldUpdateSourceUpdatedAt_WhenNewer()
    {
        var observedAt = new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

        var sourceUpdatedAt = observedAt.AddHours(-2);

        var record = new ExternalKeywordRecord(Guid.NewGuid(), "42", observedAt, sourceUpdatedAt);

        var newerSourceUpdatedAt = observedAt.AddHours(1);

        record.MarkSeen(observedAt.AddDays(1), newerSourceUpdatedAt);

        record.SourceUpdatedAt.ShouldBe(newerSourceUpdatedAt);
    }

    [Fact]
    public void MarkSeen_ShouldKeepSourceUpdatedAt_WhenIncomingValueIsOlder()
    {
        var observedAt = new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

        var sourceUpdatedAt = observedAt.AddHours(1);

        var record = new ExternalKeywordRecord(Guid.NewGuid(), "42", observedAt, sourceUpdatedAt);

        record.MarkSeen(observedAt.AddDays(1), observedAt);

        record.SourceUpdatedAt.ShouldBe(sourceUpdatedAt);
    }

    [Fact]
    public void MarkSeen_ShouldThrow_WhenObservedAtIsOlderThanLastSeenAt()
    {
        var observedAt = new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

        var record = new ExternalKeywordRecord(Guid.NewGuid(), "42", observedAt);

        var action = () => record.MarkSeen(observedAt.AddTicks(-1));

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("observedAt");
    }

    private static ExternalKeywordRecord CreateRecord()
    {
        return new ExternalKeywordRecord(Guid.NewGuid(), "42", DateTimeOffset.UtcNow);
    }
}