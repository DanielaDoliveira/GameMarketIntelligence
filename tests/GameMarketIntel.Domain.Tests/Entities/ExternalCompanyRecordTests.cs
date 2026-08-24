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
        record.LastSeenAt.ShouldBe(newerObservedAt);
    }

    [Fact]
    public void MarkSeen_ShouldNotMoveLastSeenAtBackwards()
    {
        // Arrange
        var firstSeenAt = DateTimeOffset.UtcNow;
        var record = new ExternalCompanyRecord(
            Guid.NewGuid(),
            "company-1",
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
        var record = new ExternalCompanyRecord(
            Guid.NewGuid(),
            "company-1",
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
        var record = new ExternalCompanyRecord(
            Guid.NewGuid(),
            "company-1",
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
        var record = new ExternalCompanyRecord(
            Guid.NewGuid(),
            "company-1",
            DateTimeOffset.UtcNow);

        // Act
        var action = () => record.MarkSeen(default);

        // Assert
        action.ShouldThrow<ArgumentException>();
    }
}