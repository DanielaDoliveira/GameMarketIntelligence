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
        record.FirstSeenAt.ShouldBe(observedAt);
        record.LastSeenAt.ShouldBe(observedAt);
        record.SourceUpdatedAt.ShouldBeNull();
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
        record.LastSeenAt.ShouldBe(newerObservedAt);
    }

    [Fact]
    public void MarkSeen_ShouldNotMoveLastSeenAtBackwards()
    {
        // Arrange
        var firstSeenAt = DateTimeOffset.UtcNow;
        var record = new ExternalCollectionRecord(
            Guid.NewGuid(),
            "collection-1",
            firstSeenAt);

        var olderObservedAt = firstSeenAt.AddHours(-1);

        // Act
        record.MarkSeen(olderObservedAt);

        // Assert
        record.LastSeenAt.ShouldBe(firstSeenAt);
    }

    [Fact]
    public void MarkSeen_ShouldUpdateSourceUpdatedAt_WhenSourceTimestampIsNewer()
    {
        // Arrange
        var observedAt = DateTimeOffset.UtcNow;
        var record = new ExternalCollectionRecord(
            Guid.NewGuid(),
            "collection-1",
            observedAt);

        var firstSourceUpdatedAt = observedAt.AddHours(-2);
        var newerSourceUpdatedAt = observedAt.AddHours(-1);

        record.MarkSeen(observedAt, firstSourceUpdatedAt);

        // Act
        record.MarkSeen(observedAt, newerSourceUpdatedAt);

        // Assert
        record.SourceUpdatedAt.ShouldBe(newerSourceUpdatedAt);
    }

    [Fact]
    public void MarkSeen_ShouldNotMoveSourceUpdatedAtBackwards()
    {
        // Arrange
        var observedAt = DateTimeOffset.UtcNow;
        var record = new ExternalCollectionRecord(
            Guid.NewGuid(),
            "collection-1",
            observedAt);

        var newerSourceUpdatedAt = observedAt.AddHours(-1);
        var olderSourceUpdatedAt = observedAt.AddHours(-2);

        record.MarkSeen(observedAt, newerSourceUpdatedAt);

        // Act
        record.MarkSeen(observedAt, olderSourceUpdatedAt);

        // Assert
        record.SourceUpdatedAt.ShouldBe(newerSourceUpdatedAt);
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
}