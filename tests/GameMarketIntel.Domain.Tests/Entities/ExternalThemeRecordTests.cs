using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public sealed class ExternalThemeRecordTests
{
    [Fact]
    public void Constructor_ShouldCreateUnlinkedRecord()
    {
        var dataSourceId = Guid.NewGuid();
        var observedAt = DateTimeOffset.UtcNow;

        var record = new ExternalThemeRecord(dataSourceId, " 17 ", observedAt);

        record.Id.ShouldNotBe(Guid.Empty);
        record.DataSourceId.ShouldBe(dataSourceId);
        record.ExternalId.ShouldBe("17");
        record.ThemeId.ShouldBeNull();
        record.FirstSeenAt.ShouldBe(observedAt.ToUniversalTime());
        record.LastSeenAt.ShouldBe(observedAt.ToUniversalTime());
        record.SourceUpdatedAt.ShouldBeNull();
    }

    [Fact]
    public void LinkToTheme_ShouldLinkRecord_WhenRecordIsUnlinked()
    {
        var record = CreateRecord();
        var themeId = Guid.NewGuid();

        record.LinkToTheme(themeId);

        record.ThemeId.ShouldBe(themeId);
    }

    [Fact]
    public void LinkToTheme_ShouldBeIdempotent_WhenAlreadyLinkedToSameTheme()
    {
        var record = CreateRecord();
        var themeId = Guid.NewGuid();

        record.LinkToTheme(themeId);

        record.LinkToTheme(themeId);

        record.ThemeId.ShouldBe(themeId);
    }

    [Fact]
    public void LinkToTheme_ShouldThrow_WhenAlreadyLinkedToDifferentTheme()
    {
        var record = CreateRecord();
        var firstThemeId = Guid.NewGuid();
        var secondThemeId = Guid.NewGuid();

        record.LinkToTheme(firstThemeId);

        var action = () => record.LinkToTheme(secondThemeId);

        action.ShouldThrow<InvalidOperationException>();
        record.ThemeId.ShouldBe(firstThemeId);
    }

    [Fact]
    public void LinkToTheme_ShouldThrow_WhenThemeIdIsEmpty()
    {
        var record = CreateRecord();

        var action = () => record.LinkToTheme(Guid.Empty);

        action.ShouldThrow<ArgumentException>()
            .ParamName.ShouldBe("themeId");
    }

    [Fact]
    public void Unlink_ShouldKeepRecordUnlinked_WhenAlreadyUnlinked()
    {
        var record = CreateRecord();

        record.Unlink();

        record.ThemeId.ShouldBeNull();
    }

    [Fact]
    public void Unlink_ShouldThrow_WhenRecordIsLinked()
    {
        var record = CreateRecord();
        var themeId = Guid.NewGuid();

        record.LinkToTheme(themeId);

        var action = () => record.Unlink();

        action.ShouldThrow<InvalidOperationException>();
        record.ThemeId.ShouldBe(themeId);
    }

    [Fact]
    public void MarkSeen_ShouldUpdateLastSeenAt()
    {
        var observedAt = new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

        var record = new ExternalThemeRecord(Guid.NewGuid(), "17", observedAt);

        var nextObservedAt = observedAt.AddDays(1);

        record.MarkSeen(nextObservedAt);

        record.LastSeenAt.ShouldBe(nextObservedAt);
    }

    [Fact]
    public void MarkSeen_ShouldUpdateSourceUpdatedAt_WhenNewer()
    {
        var observedAt = new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

        var sourceUpdatedAt = observedAt.AddHours(-2);

        var record = new ExternalThemeRecord(Guid.NewGuid(), "17", observedAt, sourceUpdatedAt);

        var newerSourceUpdatedAt = observedAt.AddHours(1);

        record.MarkSeen(observedAt.AddDays(1), newerSourceUpdatedAt);

        record.SourceUpdatedAt.ShouldBe(newerSourceUpdatedAt);
    }

    [Fact]
    public void MarkSeen_ShouldKeepSourceUpdatedAt_WhenIncomingValueIsOlder()
    {
        var observedAt = new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

        var sourceUpdatedAt = observedAt.AddHours(1);

        var record = new ExternalThemeRecord(Guid.NewGuid(), "17", observedAt, sourceUpdatedAt);

        record.MarkSeen(observedAt.AddDays(1), observedAt);

        record.SourceUpdatedAt.ShouldBe(sourceUpdatedAt);
    }

    [Fact]
    public void MarkSeen_ShouldThrow_WhenObservedAtIsOlderThanLastSeenAt()
    {
        var observedAt = new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

        var record = new ExternalThemeRecord(Guid.NewGuid(), "17", observedAt);

        var action = () => record.MarkSeen(
            observedAt.AddTicks(-1));

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("observedAt");
    }

    private static ExternalThemeRecord CreateRecord() =>
        new ExternalThemeRecord(Guid.NewGuid(), "17", DateTimeOffset.UtcNow);
    
}