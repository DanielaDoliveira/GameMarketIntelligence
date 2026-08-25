using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public sealed class ExternalCollectionRecordTests
{
    [Fact]
    public void Constructor_ShouldCreateExternalCollectionRecord()
    {
        // Arrange
        var dataSourceId = Guid.NewGuid();
        var observedAt = DateTimeOffset.UtcNow;

        // Act
        var record = new ExternalCollectionRecord(
            dataSourceId,
            "collection-1",
            observedAt);

        // Assert
        record.Id.ShouldNotBe(Guid.Empty);
        record.DataSourceId.ShouldBe(dataSourceId);
        record.ExternalId.ShouldBe("collection-1");
        record.CollectionId.ShouldBeNull();
        record.FirstSeenAt.ShouldBe(observedAt.ToUniversalTime());
        record.LastSeenAt.ShouldBe(observedAt.ToUniversalTime());
        record.SourceUpdatedAt.ShouldBeNull();
    }

    [Fact]
    public void Constructor_ShouldStoreSourceUpdatedAt_WhenProvided()
    {
        // Arrange
        var observedAt = DateTimeOffset.UtcNow;
        var sourceUpdatedAt = observedAt.AddHours(-1);

        // Act
        var record = new ExternalCollectionRecord(
            Guid.NewGuid(),
            "collection-1",
            observedAt,
            sourceUpdatedAt);

        // Assert
        record.SourceUpdatedAt.ShouldBe(sourceUpdatedAt.ToUniversalTime());
    }

    [Fact]
    public void Constructor_ShouldNormalizeTimestampsToUtc()
    {
        // Arrange
        var observedAt = new DateTimeOffset(
            2026,
            8,
            25,
            9,
            0,
            0,
            TimeSpan.FromHours(-3));

        var sourceUpdatedAt = observedAt.AddHours(-1);

        // Act
        var record = new ExternalCollectionRecord(
            Guid.NewGuid(),
            "collection-1",
            observedAt,
            sourceUpdatedAt);

        // Assert
        record.FirstSeenAt.Offset.ShouldBe(TimeSpan.Zero);
        record.LastSeenAt.Offset.ShouldBe(TimeSpan.Zero);
        record.SourceUpdatedAt!.Value.Offset.ShouldBe(TimeSpan.Zero);
        record.FirstSeenAt.ShouldBe(observedAt.ToUniversalTime());
        record.SourceUpdatedAt.ShouldBe(sourceUpdatedAt.ToUniversalTime());
    }

    [Fact]
    public void Constructor_ShouldTrimExternalId()
    {
        // Arrange
        var dataSourceId = Guid.NewGuid();

        // Act
        var record = new ExternalCollectionRecord(
            dataSourceId,
            "  collection-1  ",
            DateTimeOffset.UtcNow);

        // Assert
        record.ExternalId.ShouldBe("collection-1");
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenDataSourceIdIsEmpty()
    {
        // Arrange & Act
        var action = () => new ExternalCollectionRecord(
            Guid.Empty,
            "collection-1",
            DateTimeOffset.UtcNow);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowException_WhenExternalIdIsInvalid(string? externalId)
    {
        // Arrange & Act
        var action = () => new ExternalCollectionRecord(
            Guid.NewGuid(),
            externalId!,
            DateTimeOffset.UtcNow);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldAllowExternalIdAtMaximumLength()
    {
        // Arrange
        var externalId = new string('A', ExternalCollectionRecord.MaxExternalIdLength);

        // Act
        var record = new ExternalCollectionRecord(
            Guid.NewGuid(),
            externalId,
            DateTimeOffset.UtcNow);

        // Assert
        record.ExternalId.Length.ShouldBe(ExternalCollectionRecord.MaxExternalIdLength);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenExternalIdExceedsMaximumLength()
    {
        // Arrange
        var externalId = new string('A', ExternalCollectionRecord.MaxExternalIdLength + 1);

        // Act
        var action = () => new ExternalCollectionRecord(
            Guid.NewGuid(),
            externalId,
            DateTimeOffset.UtcNow);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenObservedAtIsDefault()
    {
        // Arrange & Act
        var action = () => new ExternalCollectionRecord(
            Guid.NewGuid(),
            "collection-1",
            default);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenSourceUpdatedAtIsDefault()
    {
        // Arrange & Act
        var action = () => new ExternalCollectionRecord(
            Guid.NewGuid(),
            "collection-1",
            DateTimeOffset.UtcNow,
            default(DateTimeOffset));

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void LinkToCollection_ShouldLinkRecord()
    {
        // Arrange
        var record = new ExternalCollectionRecord(
            Guid.NewGuid(),
            "collection-1",
            DateTimeOffset.UtcNow);

        var collectionId = Guid.NewGuid();

        // Act
        record.LinkToCollection(collectionId);

        // Assert
        record.CollectionId.ShouldBe(collectionId);
    }

    [Fact]
    public void LinkToCollection_ShouldThrowException_WhenCollectionIdIsEmpty()
    {
        // Arrange
        var record = new ExternalCollectionRecord(
            Guid.NewGuid(),
            "collection-1",
            DateTimeOffset.UtcNow);

        // Act
        var action = () => record.LinkToCollection(Guid.Empty);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void LinkToCollection_ShouldAllowLinkingAgainToTheSameCollection()
    {
        // Arrange
        var record = new ExternalCollectionRecord(
            Guid.NewGuid(),
            "collection-1",
            DateTimeOffset.UtcNow);

        var collectionId = Guid.NewGuid();
        record.LinkToCollection(collectionId);

        // Act
        record.LinkToCollection(collectionId);

        // Assert
        record.CollectionId.ShouldBe(collectionId);
    }

    [Fact]
    public void LinkToCollection_ShouldThrowException_WhenRecordIsAlreadyLinkedToAnotherCollection()
    {
        // Arrange
        var record = new ExternalCollectionRecord(
            Guid.NewGuid(),
            "collection-1",
            DateTimeOffset.UtcNow);

        record.LinkToCollection(Guid.NewGuid());

        // Act
        var action = () => record.LinkToCollection(Guid.NewGuid());

        // Assert
        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void MarkSeen_ShouldUpdateLastSeenAt_WhenObservedAtIsNewer()
    {
        // Arrange
        var firstSeenAt = DateTimeOffset.UtcNow;
        var record = new ExternalCollectionRecord(
            Guid.NewGuid(),
            "collection-1",
            firstSeenAt);

        var newerObservedAt = firstSeenAt.AddHours(1);

        // Act
        record.MarkSeen(newerObservedAt);

        // Assert
        record.LastSeenAt.ShouldBe(newerObservedAt.ToUniversalTime());
    }

    [Fact]
    public void MarkSeen_ShouldThrowException_WhenObservedAtIsOlderThanLastSeenAt()
    {
        // Arrange
        var firstSeenAt = DateTimeOffset.UtcNow;
        var record = new ExternalCollectionRecord(
            Guid.NewGuid(),
            "collection-1",
            firstSeenAt);

        var olderObservedAt = firstSeenAt.AddTicks(-1);

        // Act
        var action = () => record.MarkSeen(olderObservedAt);

        // Assert
        action.ShouldThrow<ArgumentException>()
            .ParamName.ShouldBe("observedAt");
    }

    [Fact]
    public void MarkSeen_ShouldUpdateSourceUpdatedAt_WhenSourceTimestampIsNewer()
    {
        // Arrange
        var observedAt = DateTimeOffset.UtcNow;
        var firstSourceUpdatedAt = observedAt.AddHours(-2);
        var newerSourceUpdatedAt = observedAt.AddHours(-1);

        var record = new ExternalCollectionRecord(
            Guid.NewGuid(),
            "collection-1",
            observedAt,
            firstSourceUpdatedAt);

        // Act
        record.MarkSeen(observedAt.AddHours(1), newerSourceUpdatedAt);

        // Assert
        record.SourceUpdatedAt.ShouldBe(newerSourceUpdatedAt.ToUniversalTime());
    }

    [Fact]
    public void MarkSeen_ShouldNotMoveSourceUpdatedAtBackwards()
    {
        // Arrange
        var observedAt = DateTimeOffset.UtcNow;
        var newerSourceUpdatedAt = observedAt.AddHours(-1);
        var olderSourceUpdatedAt = observedAt.AddHours(-2);

        var record = new ExternalCollectionRecord(
            Guid.NewGuid(),
            "collection-1",
            observedAt,
            newerSourceUpdatedAt);

        // Act
        record.MarkSeen(observedAt.AddHours(1), olderSourceUpdatedAt);

        // Assert
        record.SourceUpdatedAt.ShouldBe(newerSourceUpdatedAt.ToUniversalTime());
    }

    [Fact]
    public void MarkSeen_ShouldNormalizeTimestampsToUtc()
    {
        // Arrange
        var firstObservedAt = new DateTimeOffset(
            2026,
            8,
            25,
            9,
            0,
            0,
            TimeSpan.FromHours(-3));

        var record = new ExternalCollectionRecord(
            Guid.NewGuid(),
            "collection-1",
            firstObservedAt);

        var nextObservedAt = firstObservedAt.AddHours(1);
        var sourceUpdatedAt = firstObservedAt.AddMinutes(30);

        // Act
        record.MarkSeen(nextObservedAt, sourceUpdatedAt);

        // Assert
        record.LastSeenAt.Offset.ShouldBe(TimeSpan.Zero);
        record.SourceUpdatedAt!.Value.Offset.ShouldBe(TimeSpan.Zero);
        record.LastSeenAt.ShouldBe(nextObservedAt.ToUniversalTime());
        record.SourceUpdatedAt.ShouldBe(sourceUpdatedAt.ToUniversalTime());
    }

    [Fact]
    public void MarkSeen_ShouldThrowException_WhenObservedAtIsDefault()
    {
        // Arrange
        var record = new ExternalCollectionRecord(
            Guid.NewGuid(),
            "collection-1",
            DateTimeOffset.UtcNow);

        // Act
        var action = () => record.MarkSeen(default);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void MarkSeen_ShouldThrowException_WhenSourceUpdatedAtIsDefault()
    {
        // Arrange
        var observedAt = DateTimeOffset.UtcNow;
        var record = new ExternalCollectionRecord(
            Guid.NewGuid(),
            "collection-1",
            observedAt);

        // Act
        var action = () => record.MarkSeen(
            observedAt.AddHours(1),
            default(DateTimeOffset));

        // Assert
        action.ShouldThrow<ArgumentException>();
    }
}

