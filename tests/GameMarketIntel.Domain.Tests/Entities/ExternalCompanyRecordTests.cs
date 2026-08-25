using GameMarketIntel.Domain.Entities;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public sealed class ExternalCompanyRecordTests
{
    [Fact]
    public void Constructor_ShouldCreateExternalCompanyRecord()
    {
        // Arrange
        var dataSourceId = Guid.NewGuid();
        var observedAt = DateTimeOffset.UtcNow;

        // Act
        var record = new ExternalCompanyRecord(
            dataSourceId,
            "company-1",
            observedAt);

        // Assert
        record.Id.ShouldNotBe(Guid.Empty);
        record.DataSourceId.ShouldBe(dataSourceId);
        record.ExternalId.ShouldBe("company-1");
        record.CompanyId.ShouldBeNull();
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
        var record = new ExternalCompanyRecord(
            Guid.NewGuid(),
            "company-1",
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
        var record = new ExternalCompanyRecord(
            Guid.NewGuid(),
            "company-1",
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
        var record = new ExternalCompanyRecord(
            dataSourceId,
            "  company-1  ",
            DateTimeOffset.UtcNow);

        // Assert
        record.ExternalId.ShouldBe("company-1");
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenDataSourceIdIsEmpty()
    {
        // Arrange & Act
        var action = () => new ExternalCompanyRecord(
            Guid.Empty,
            "company-1",
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
        var action = () => new ExternalCompanyRecord(
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
        var externalId = new string('A', ExternalCompanyRecord.MaxExternalIdLength);

        // Act
        var record = new ExternalCompanyRecord(
            Guid.NewGuid(),
            externalId,
            DateTimeOffset.UtcNow);

        // Assert
        record.ExternalId.Length.ShouldBe(ExternalCompanyRecord.MaxExternalIdLength);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenExternalIdExceedsMaximumLength()
    {
        // Arrange
        var externalId = new string('A', ExternalCompanyRecord.MaxExternalIdLength + 1);

        // Act
        var action = () => new ExternalCompanyRecord(
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
        var action = () => new ExternalCompanyRecord(
            Guid.NewGuid(),
            "company-1",
            default);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenSourceUpdatedAtIsDefault()
    {
        // Arrange & Act
        var action = () => new ExternalCompanyRecord(
            Guid.NewGuid(),
            "company-1",
            DateTimeOffset.UtcNow,
            default(DateTimeOffset));

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void LinkToCompany_ShouldLinkRecord()
    {
        // Arrange
        var record = new ExternalCompanyRecord(
            Guid.NewGuid(),
            "company-1",
            DateTimeOffset.UtcNow);

        var companyId = Guid.NewGuid();

        // Act
        record.LinkToCompany(companyId);

        // Assert
        record.CompanyId.ShouldBe(companyId);
    }

    [Fact]
    public void LinkToCompany_ShouldThrowException_WhenCompanyIdIsEmpty()
    {
        // Arrange
        var record = new ExternalCompanyRecord(
            Guid.NewGuid(),
            "company-1",
            DateTimeOffset.UtcNow);

        // Act
        var action = () => record.LinkToCompany(Guid.Empty);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void LinkToCompany_ShouldAllowLinkingAgainToTheSameCompany()
    {
        // Arrange
        var record = new ExternalCompanyRecord(
            Guid.NewGuid(),
            "company-1",
            DateTimeOffset.UtcNow);

        var companyId = Guid.NewGuid();
        record.LinkToCompany(companyId);

        // Act
        record.LinkToCompany(companyId);

        // Assert
        record.CompanyId.ShouldBe(companyId);
    }

    [Fact]
    public void LinkToCompany_ShouldThrowException_WhenRecordIsAlreadyLinkedToAnotherCompany()
    {
        // Arrange
        var record = new ExternalCompanyRecord(
            Guid.NewGuid(),
            "company-1",
            DateTimeOffset.UtcNow);

        record.LinkToCompany(Guid.NewGuid());

        // Act
        var action = () => record.LinkToCompany(Guid.NewGuid());

        // Assert
        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void MarkSeen_ShouldUpdateLastSeenAt_WhenObservedAtIsNewer()
    {
        // Arrange
        var firstSeenAt = DateTimeOffset.UtcNow;
        var record = new ExternalCompanyRecord(
            Guid.NewGuid(),
            "company-1",
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
        var record = new ExternalCompanyRecord(
            Guid.NewGuid(),
            "company-1",
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

        var record = new ExternalCompanyRecord(
            Guid.NewGuid(),
            "company-1",
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

        var record = new ExternalCompanyRecord(
            Guid.NewGuid(),
            "company-1",
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

        var record = new ExternalCompanyRecord(
            Guid.NewGuid(),
            "company-1",
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
        var record = new ExternalCompanyRecord(
            Guid.NewGuid(),
            "company-1",
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
        var record = new ExternalCompanyRecord(
            Guid.NewGuid(),
            "company-1",
            observedAt);

        // Act
        var action = () => record.MarkSeen(
            observedAt.AddHours(1),
            default(DateTimeOffset));

        // Assert
        action.ShouldThrow<ArgumentException>();
    }
}