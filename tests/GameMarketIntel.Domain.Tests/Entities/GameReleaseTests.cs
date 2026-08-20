using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;
using Shouldly;

namespace GameMarketIntel.Domain.Tests.Entities;

public sealed class GameReleaseTests
{
    private const string ExternalReleaseId = "release-144542-switch";

    [Fact]
    public void Create_ShouldCreateContextualRelease()
    {
        var gameId = Guid.NewGuid();
        var platformId = Guid.NewGuid();
        var externalGameRecord = CreateLinkedExternalGameRecord(gameId);
        var releaseDate = ReleaseDateValue.ForDay(
            new DateOnly(2026, 8, 18));
        var observedAt = CreateObservedAt();

        var release =
            GameRelease.Create
            (
                gameId,
                platformId,
                externalGameRecord,
                ExternalReleaseId,
                releaseDate,
                observedAt
            );

        release.Id.ShouldNotBe(Guid.Empty);
        release.GameId.ShouldBe(gameId);
        release.PlatformId.ShouldBe(platformId);
        release.ExternalGameRecordId.ShouldBe(externalGameRecord.Id);
        release.ExternalReleaseId.ShouldBe(ExternalReleaseId);
        release.ReleaseDate.ShouldBe(releaseDate);
        release.ObservedAt.ShouldBe(observedAt);
        release.RegionCode.ShouldBeNull();
        release.Status.ShouldBeNull();
        release.Ecosystem.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldTrimExternalReleaseId()
    {
        var gameId = Guid.NewGuid();
        var externalGameRecord = CreateLinkedExternalGameRecord(gameId);

        var release = GameRelease.Create(
            gameId,
            Guid.NewGuid(),
            externalGameRecord,
            $" {ExternalReleaseId} ",
            ReleaseDateValue.ForYear(2026),
            CreateObservedAt());

        release.ExternalReleaseId.ShouldBe(ExternalReleaseId);
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenExternalReleaseIdIsWhiteSpace()
    {
        var gameId = Guid.NewGuid();
        var externalGameRecord = CreateLinkedExternalGameRecord(gameId);

        var exception = Should.Throw<ArgumentException>
        (() =>
            GameRelease.Create
            (
                gameId,
                Guid.NewGuid(),
                externalGameRecord,
                externalReleaseId: " ",
                releaseDate: ReleaseDateValue.ForYear(2026),
                observedAt: CreateObservedAt()
            )
        );

        exception.ParamName.ShouldBe("externalReleaseId");
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenExternalReleaseIdExceedsMaximumLength()
    {
        var gameId = Guid.NewGuid();
        var externalGameRecord = CreateLinkedExternalGameRecord(gameId);

        var externalReleaseId =
            new string
            (
                'R',
                GameRelease.MaximumExternalReleaseIdLength + 1
            );

        var exception = Should.Throw<ArgumentException>
        (() =>
            GameRelease.Create
            (
                gameId,
                Guid.NewGuid(),
                externalGameRecord,
                externalReleaseId,
                ReleaseDateValue.ForYear(2026),
                CreateObservedAt()
            )
        );

        exception.ParamName.ShouldBe("externalReleaseId");
    }

    [Fact]
    public void SetStatus_ShouldSetReleaseStatus()
    {
        var release = CreateRelease();

        release.SetStatus(GameReleaseStatus.Announced);

        release.Status.ShouldBe(GameReleaseStatus.Announced);
    }

    [Fact]
    public void SetRegionCode_ShouldNormalizeRegionCode()
    {
        var release = CreateRelease();

        release.SetRegionCode(" br ");

        release.RegionCode.ShouldBe("BR");
    }

    [Fact]
    public void SetEcosystem_ShouldTrimEcosystem()
    {
        var release = CreateRelease();

        release.SetEcosystem(" Nintendo eShop ");

        release.Ecosystem.ShouldBe("Nintendo eShop");
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenGameIdIsEmpty()
    {
        var externalGameRecord = CreateLinkedExternalGameRecord(Guid.NewGuid());

        var exception = Should.Throw<ArgumentException>
        (() =>
            GameRelease.Create
            (
                Guid.Empty,
                Guid.NewGuid(),
                externalGameRecord,
                ExternalReleaseId,
                ReleaseDateValue.ForYear(2026),
                CreateObservedAt()
            )
        );

        exception.ParamName.ShouldBe("gameId");
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenPlatformIdIsEmpty()
    {
        var gameId = Guid.NewGuid();
        var externalGameRecord = CreateLinkedExternalGameRecord(gameId);

        var exception = Should.Throw<ArgumentException>
        (() =>
            GameRelease.Create
            (
                gameId,
                Guid.Empty,
                externalGameRecord,
                ExternalReleaseId,
                ReleaseDateValue.ForYear(2026),
                CreateObservedAt()
            )
        );

        exception.ParamName.ShouldBe("platformId");
    }

    [Fact]
    public void Create_ShouldThrowArgumentNullException_WhenExternalGameRecordIsNull()
    {
        var exception = Should.Throw<ArgumentNullException>
        (() =>
            GameRelease.Create
            (
                Guid.NewGuid(),
                Guid.NewGuid(),
                null!,
                ExternalReleaseId,
                ReleaseDateValue.ForYear(2026),
                CreateObservedAt()
            )
        );

        exception.ParamName.ShouldBe("externalGameRecord");
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenExternalGameRecordIsNotLinked()
    {
        var externalGameRecord = new ExternalGameRecord
        (
            Guid.NewGuid(),
            "144542",
            CreateObservedAt()
        );

        var exception = Should.Throw<ArgumentException>
        (() =>
            GameRelease.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                externalGameRecord,
                ExternalReleaseId,
                ReleaseDateValue.ForYear(2026),
                CreateObservedAt()
            )
        );

        exception.ParamName.ShouldBe("externalGameRecord");
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenExternalGameRecordIsLinkedToDifferentGame()
    {
        var gameId = Guid.NewGuid();
        var externalGameRecord = CreateLinkedExternalGameRecord(Guid.NewGuid());

        var exception = Should.Throw<ArgumentException>
        (() =>
            GameRelease.Create(
                gameId,
                Guid.NewGuid(),
                externalGameRecord,
                ExternalReleaseId,
                ReleaseDateValue.ForYear(2026),
                CreateObservedAt())
        );

        exception.ParamName.ShouldBe("externalGameRecord");
    }

    [Fact]
    public void Create_ShouldThrowArgumentNullException_WhenReleaseDateIsNull()
    {
        var gameId = Guid.NewGuid();
        var externalGameRecord = CreateLinkedExternalGameRecord(gameId);

        var exception = Should.Throw<ArgumentNullException>
        (() =>
            GameRelease.Create
            (
                gameId,
                Guid.NewGuid(),
                externalGameRecord,
                ExternalReleaseId,
                releaseDate: null!,
                observedAt: CreateObservedAt())
        );

        exception.ParamName.ShouldBe("releaseDate");
    }

    [Fact]
    public void SetRegionCode_ShouldClearRegionCode_WhenValueIsWhiteSpace()
    {
        var release = CreateRelease();
        release.SetRegionCode("BR");

        release.SetRegionCode(" ");

        release.RegionCode.ShouldBeNull();
    }

    [Fact]
    public void SetRegionCode_ShouldThrowArgumentException_WhenMaximumLengthIsExceeded()
    {
        var release = CreateRelease();
        var regionCode = new string('R', GameRelease.MaximumRegionCodeLength + 1);

        var exception = Should.Throw<ArgumentException>(() => release.SetRegionCode(regionCode));

        exception.ParamName.ShouldBe("regionCode");
    }

    [Fact]
    public void SetEcosystem_ShouldClearEcosystem_WhenValueIsWhiteSpace()
    {
        var release = CreateRelease();
        release.SetEcosystem("Nintendo eShop");

        release.SetEcosystem(" ");

        release.Ecosystem.ShouldBeNull();
    }

    [Fact]
    public void SetEcosystem_ShouldThrowArgumentException_WhenMaximumLengthIsExceeded()
    {
        var release = CreateRelease();
        var ecosystem = new string('E', GameRelease.MaximumEcosystemLength + 1);

        var exception = Should.Throw<ArgumentException>(() => release.SetEcosystem(ecosystem));

        exception.ParamName.ShouldBe("ecosystem");
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenObservedAtIsDefault()
    {
        var gameId = Guid.NewGuid();
        var externalGameRecord = CreateLinkedExternalGameRecord(gameId);

        var exception = Should.Throw<ArgumentException>(() =>
            GameRelease.Create(
                gameId,
                Guid.NewGuid(),
                externalGameRecord,
                ExternalReleaseId,
                ReleaseDateValue.ForYear(2026),
                observedAt: default));

        exception.ParamName.ShouldBe("observedAt");
    }

    [Fact]
    public void SetStatus_ShouldThrowArgumentOutOfRangeException_WhenStatusIsInvalid()
    {
        var release = CreateRelease();
        var invalidStatus = (GameReleaseStatus)int.MaxValue;

        var exception = Should.Throw<ArgumentOutOfRangeException>(() =>
            release.SetStatus(invalidStatus));

        exception.ParamName.ShouldBe("status");
    }

    [Fact]
    public void SetStatus_ShouldClearStatus_WhenStatusIsNull()
    {
        var release = CreateRelease();
        release.SetStatus(GameReleaseStatus.Announced);

        release.SetStatus(null);

        release.Status.ShouldBeNull();
    }

    [Fact]
    public void UpdateReleaseDate_ShouldUpdateDateAndObservedAt_WhenObservationIsNewer()
    {
        var release = CreateRelease();
        var updatedReleaseDate = ReleaseDateValue.ForQuarter(2027, 2);
        var updatedObservedAt = release.ObservedAt.AddDays(1);

        release.UpdateReleaseDate(updatedReleaseDate, updatedObservedAt);

        release.ReleaseDate.ShouldBe(updatedReleaseDate);
        release.ObservedAt.ShouldBe(updatedObservedAt);
    }

    [Fact]
    public void UpdateReleaseDate_ShouldAllowObservationAtSameInstant()
    {
        var release = CreateRelease();
        var updatedReleaseDate = ReleaseDateValue.ForMonth(2026, 10);

        release.UpdateReleaseDate(updatedReleaseDate, release.ObservedAt);

        release.ReleaseDate.ShouldBe(updatedReleaseDate);
    }

    [Fact]
    public void UpdateReleaseDate_ShouldThrowArgumentException_WhenObservationIsOlder()
    {
        var release = CreateRelease();
        var olderObservedAt = release.ObservedAt.AddTicks(-1);

        var exception = Should.Throw<ArgumentException>
        (() =>
            release.UpdateReleaseDate(ReleaseDateValue.ForYear(2027), olderObservedAt)
        );

        exception.ParamName.ShouldBe("observedAt");
    }

    [Fact]
    public void UpdateReleaseDate_ShouldThrowArgumentNullException_WhenReleaseDateIsNull()
    {
        var release = CreateRelease();

        var exception = Should.Throw<ArgumentNullException>(() =>
            release.UpdateReleaseDate
            (
                releaseDate: null!,
                release.ObservedAt)
        );

        exception.ParamName.ShouldBe("releaseDate");
    }

    [Fact]
    public void UpdateReleaseDate_ShouldThrowArgumentException_WhenObservedAtIsDefault()
    {
        var release = CreateRelease();

        var exception = Should.Throw<ArgumentException>
        (() =>
            release.UpdateReleaseDate
            (
                ReleaseDateValue.ForYear(2027),
                observedAt: default
            )
        );

        exception.ParamName.ShouldBe("observedAt");
    }

    private static GameRelease CreateRelease()
    {
        var gameId = Guid.NewGuid();
        var externalGameRecord = CreateLinkedExternalGameRecord(gameId);

        return GameRelease.Create
        (
            gameId,
            Guid.NewGuid(),
            externalGameRecord,
            ExternalReleaseId,
            ReleaseDateValue.ForYear(2026),
            CreateObservedAt()
        );
    }

    private static ExternalGameRecord CreateLinkedExternalGameRecord(Guid gameId)
    {
        var externalGameRecord = new ExternalGameRecord(Guid.NewGuid(), "144542", CreateObservedAt());

        externalGameRecord.LinkToGame(gameId);

        return externalGameRecord;
    }

    private static DateTimeOffset CreateObservedAt()
    {
        return new DateTimeOffset(
            2026,
            8,
            18,
            12,
            0,
            0,
            TimeSpan.Zero);
    }
}