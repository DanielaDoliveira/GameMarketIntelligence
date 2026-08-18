using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public sealed class ExternalGameRecordTests
{
    private static readonly DateTimeOffset ObservedAt =
        new(2026, 8, 18, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_ShouldCreateUnlinkedRecord_WhenDataIsValid()
    {
        // Arrange
        var dataSourceId = Guid.NewGuid();

        // Act
        var record = new ExternalGameRecord(
            dataSourceId,
            " 144542 ",
            ObservedAt);

        // Assert
        record.Id.ShouldNotBe(Guid.Empty);
        record.DataSourceId.ShouldBe(dataSourceId);
        record.ExternalId.ShouldBe("144542");
        record.GameId.ShouldBeNull();
        record.Status.ShouldBe(ExternalGameRecordStatus.Unlinked);
        record.FirstSeenAt.ShouldBe(ObservedAt);
        record.LastSeenAt.ShouldBe(ObservedAt);
        record.SourceUpdatedAt.ShouldBeNull();
    }

    [Fact]
    public void Constructor_ShouldPreserveExternalIdCase()
    {
        // Act
        var record = new ExternalGameRecord(
            Guid.NewGuid(),
            "QAbC123",
            ObservedAt);

        // Assert
        record.ExternalId.ShouldBe("QAbC123");
    }

    [Fact]
    public void Constructor_ShouldNormalizeTimestampsToUtc()
    {
        // Arrange
        var observedAt = new DateTimeOffset(
            2026,
            8,
            18,
            9,
            0,
            0,
            TimeSpan.FromHours(-3));

        // Act
        var record = new ExternalGameRecord(
            Guid.NewGuid(),
            "144542",
            observedAt);

        // Assert
        record.FirstSeenAt.ShouldBe(ObservedAt);
        record.LastSeenAt.ShouldBe(ObservedAt);
        record.FirstSeenAt.Offset.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenDataSourceIdIsEmpty()
    {
        // Act
        var exception = Should.Throw<ArgumentException>(() =>
        {
            _ = new ExternalGameRecord(
                Guid.Empty,
                "144542",
                ObservedAt);
        });

        // Assert
        exception.ParamName.ShouldBe("dataSourceId");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowArgumentException_WhenExternalIdIsMissing(
        string invalidExternalId)
    {
        // Act
        var exception = Should.Throw<ArgumentException>(() =>
        {
            _ = new ExternalGameRecord(
                Guid.NewGuid(),
                invalidExternalId,
                ObservedAt);
        });

        // Assert
        exception.ParamName.ShouldBe("externalId");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenExternalIdIsTooLong()
    {
        // Arrange
        var invalidExternalId = new string(
            'a',
            ExternalGameRecord.MaxExternalIdLength + 1);

        // Act
        var exception = Should.Throw<ArgumentException>(() =>
        {
            _ = new ExternalGameRecord(
                Guid.NewGuid(),
                invalidExternalId,
                ObservedAt);
        });

        // Assert
        exception.ParamName.ShouldBe("externalId");
    }

    [Fact]
    public void LinkToGame_ShouldAssociateCanonicalGame()
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
    public void LinkToGame_ShouldThrowArgumentException_WhenGameIdIsEmpty()
    {
        // Arrange
        var record = CreateRecord();

        // Act
        var exception = Should.Throw<ArgumentException>(() =>
        {
            record.LinkToGame(Guid.Empty);
        });

        // Assert
        exception.ParamName.ShouldBe("gameId");
    }

    [Fact]
    public void Unlink_ShouldRemoveCanonicalGameAssociation()
    {
        // Arrange
        var record = CreateRecord();
        record.LinkToGame(Guid.NewGuid());

        // Act
        record.Unlink();

        // Assert
        record.GameId.ShouldBeNull();
        record.Status.ShouldBe(ExternalGameRecordStatus.Unlinked);
    }

    [Fact]
    public void Reject_ShouldRemoveAssociationAndMarkRecordAsRejected()
    {
        // Arrange
        var record = CreateRecord();
        record.LinkToGame(Guid.NewGuid());

        // Act
        record.Reject();

        // Assert
        record.GameId.ShouldBeNull();
        record.Status.ShouldBe(ExternalGameRecordStatus.Rejected);
    }
    
    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenObservedAtIsDefault()
    {
        // Act
        var exception = Should.Throw<ArgumentException>(() =>
        {
            _ = new ExternalGameRecord(
                Guid.NewGuid(),
                "144542",
                default);
        });

        // Assert
        exception.ParamName.ShouldBe("observedAt");
    }

    [Fact]
    public void MarkSeen_ShouldUpdateLastSeenAtAndSourceUpdatedAt()
    {
        // Arrange
        var record = CreateRecord();

        var nextObservation = ObservedAt.AddHours(2);
        var sourceUpdatedAt = ObservedAt.AddHours(1);

        // Act
        record.MarkSeen(
            nextObservation,
            sourceUpdatedAt);

        // Assert
        record.LastSeenAt.ShouldBe(nextObservation);
        record.SourceUpdatedAt.ShouldBe(sourceUpdatedAt);
    }

    [Fact]
    public void MarkSeen_ShouldThrowArgumentException_WhenObservationRegresses()
    {
        // Arrange
        var record = CreateRecord();
        var earlierObservation = ObservedAt.AddMinutes(-1);

        // Act
        var exception = Should.Throw<ArgumentException>(() =>
        {
            record.MarkSeen(earlierObservation);
        });

        // Assert
        exception.ParamName.ShouldBe("observedAt");
    }

    private static ExternalGameRecord CreateRecord()
    {
        return new ExternalGameRecord(
            Guid.NewGuid(),
            "144542",
            ObservedAt);
    }
}