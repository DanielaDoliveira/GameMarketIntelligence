using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public class ExternalKeywordRecordTests
{
    [Fact]
    public void Constructor_ShouldInitializeExternalKeywordRecord()
    {
        // Arrange
        var dataSourceId = Guid.NewGuid();
        var observedAt = new DateTimeOffset(2026, 8, 19, 12, 0, 0, TimeSpan.FromHours(-3));

        // Act
        var record = new ExternalKeywordRecord(dataSourceId, "  42  ", observedAt);

        // Assert
        record.Id.ShouldNotBe(Guid.Empty);
        record.DataSourceId.ShouldBe(dataSourceId);
        record.ExternalId.ShouldBe("42");
        record.KeywordId.ShouldBeNull();
        record.FirstSeenAt.ShouldBe(observedAt.ToUniversalTime());
        record.LastSeenAt.ShouldBe(observedAt.ToUniversalTime());
        record.SourceUpdatedAt.ShouldBeNull();
    }

    [Fact]
    public void Constructor_ShouldStoreNormalizedSourceUpdatedAt()
    {
        // Arrange
        var dataSourceId = Guid.NewGuid();
        var observedAt = new DateTimeOffset(2026, 8, 19, 12, 0, 0, TimeSpan.FromHours(-3));
        var sourceUpdatedAt = new DateTimeOffset(2026, 8, 18, 21, 30, 0, TimeSpan.FromHours(-3));

        // Act
        var record = new ExternalKeywordRecord(dataSourceId, "42", observedAt, sourceUpdatedAt);

        // Assert
        record.SourceUpdatedAt.ShouldBe(sourceUpdatedAt.ToUniversalTime());
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenDataSourceIdIsEmpty()
    {
        // Act
        var action = () => new ExternalKeywordRecord(Guid.Empty, "42", DateTimeOffset.UtcNow);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowException_WhenExternalIdIsInvalid(string? externalId)
    {
        // Act
        var action = () => new ExternalKeywordRecord(Guid.NewGuid(), externalId!, DateTimeOffset.UtcNow);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldAcceptExternalIdAtMaximumLength()
    {
        // Arrange
        var externalId = new string('A', ExternalKeywordRecord.MaxExternalIdLength);

        // Act
        var record = new ExternalKeywordRecord(Guid.NewGuid(), externalId, DateTimeOffset.UtcNow);

        // Assert
        record.ExternalId.Length.ShouldBe(ExternalKeywordRecord.MaxExternalIdLength);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenExternalIdExceedsMaximumLength()
    {
        // Arrange
        var externalId = new string('A', ExternalKeywordRecord.MaxExternalIdLength + 1);

        // Act
        var action = () => new ExternalKeywordRecord(Guid.NewGuid(), externalId, DateTimeOffset.UtcNow);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenObservedAtIsDefault()
    {
        // Act
        var action = () => new ExternalKeywordRecord(Guid.NewGuid(), "42", default);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void LinkToKeyword_ShouldSetKeywordId()
    {
        // Arrange
        var record = new ExternalKeywordRecord(Guid.NewGuid(), "42", DateTimeOffset.UtcNow);
        var keywordId = Guid.NewGuid();

        // Act
        record.LinkToKeyword(keywordId);

        // Assert
        record.KeywordId.ShouldBe(keywordId);
    }

    [Fact]
    public void LinkToKeyword_ShouldThrowException_WhenKeywordIdIsEmpty()
    {
        // Arrange
        var record = new ExternalKeywordRecord(Guid.NewGuid(), "42", DateTimeOffset.UtcNow);

        // Act
        var action = () => record.LinkToKeyword(Guid.Empty);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Unlink_ShouldClearKeywordId()
    {
        // Arrange
        var record = new ExternalKeywordRecord(Guid.NewGuid(), "42", DateTimeOffset.UtcNow);
        record.LinkToKeyword(Guid.NewGuid());

        // Act
        record.Unlink();

        // Assert
        record.KeywordId.ShouldBeNull();
    }

    [Fact]
    public void MarkSeen_ShouldUpdateLastSeenAt()
    {
        // Arrange
        var firstObservedAt = new DateTimeOffset(2026, 8, 19, 10, 0, 0, TimeSpan.Zero);
        var nextObservedAt = firstObservedAt.AddHours(2);
        var record = new ExternalKeywordRecord(Guid.NewGuid(), "42", firstObservedAt);

        // Act
        record.MarkSeen(nextObservedAt);

        // Assert
        record.LastSeenAt.ShouldBe(nextObservedAt.ToUniversalTime());
    }

    [Fact]
    public void MarkSeen_ShouldThrowException_WhenObservedAtIsEarlierThanLastSeenAt()
    {
        // Arrange
        var firstObservedAt = new DateTimeOffset(2026, 8, 19, 10, 0, 0, TimeSpan.Zero);
        var record = new ExternalKeywordRecord(Guid.NewGuid(), "42", firstObservedAt);

        // Act
        var action = () => record.MarkSeen(firstObservedAt.AddMinutes(-1));

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void MarkSeen_ShouldUpdateSourceUpdatedAt_WhenNewValueIsMoreRecent()
    {
        // Arrange
        var observedAt = new DateTimeOffset(2026, 8, 19, 10, 0, 0, TimeSpan.Zero);
        var sourceUpdatedAt = observedAt.AddDays(-2);
        var newerSourceUpdatedAt = observedAt.AddDays(-1);
        var record = new ExternalKeywordRecord(Guid.NewGuid(), "42", observedAt, sourceUpdatedAt);

        // Act
        record.MarkSeen(observedAt.AddHours(1), newerSourceUpdatedAt);

        // Assert
        record.SourceUpdatedAt.ShouldBe(newerSourceUpdatedAt.ToUniversalTime());
    }

    [Fact]
    public void MarkSeen_ShouldKeepSourceUpdatedAt_WhenNewValueIsOlder()
    {
        // Arrange
        var observedAt = new DateTimeOffset(
            2026, 8, 19, 10, 0, 0, TimeSpan.Zero);
        var sourceUpdatedAt = observedAt.AddDays(-1);
        var olderSourceUpdatedAt = observedAt.AddDays(-2);
        var record = new ExternalKeywordRecord(Guid.NewGuid(), "42", observedAt, sourceUpdatedAt);

        // Act
        record.MarkSeen(observedAt.AddHours(1), olderSourceUpdatedAt);

        // Assert
        record.SourceUpdatedAt.ShouldBe(sourceUpdatedAt.ToUniversalTime());
    }
}