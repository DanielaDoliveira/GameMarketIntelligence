using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;

namespace GameMarketIntel.Domain.Entities;

public sealed class GameRelease
{
    public const int MaximumRegionCodeLength = 20;
    public const int MaximumEcosystemLength = 100;
    public const int MaximumExternalReleaseIdLength = 100;

    public Guid Id { get; private set; }

    public Guid GameId { get; private set; }

    public Guid PlatformId { get; private set; }

    public Guid ExternalGameRecordId { get; private set; }

    public ReleaseDateValue ReleaseDate { get; private set; } = null!;

    public string? RegionCode { get; private set; }

    public GameReleaseStatus? Status { get; private set; }

    public string? Ecosystem { get; private set; }

    public DateTimeOffset ObservedAt { get; private set; }

    public string ExternalReleaseId { get; private set; } = null!;

    private GameRelease()
    {
    }

    private GameRelease(Guid id, Guid gameId, Guid platformId, Guid externalGameRecordId, string externalReleaseId, ReleaseDateValue releaseDate, DateTimeOffset observedAt)
    {
        Id = id;
        GameId = gameId;
        PlatformId = platformId;
        ExternalGameRecordId = externalGameRecordId;
        ExternalReleaseId = externalReleaseId;
        ReleaseDate = releaseDate;
        ObservedAt = observedAt;
    }

    public static GameRelease Create(Guid gameId, Guid platformId, ExternalGameRecord externalGameRecord, string externalReleaseId, ReleaseDateValue releaseDate, DateTimeOffset observedAt)
    {
        ValidateRequiredId(gameId, nameof(gameId));
        ValidateRequiredId(platformId, nameof(platformId));

        ArgumentNullException.ThrowIfNull(externalGameRecord);

        if (!externalGameRecord.GameId.HasValue)
            throw new ArgumentException
            (
                "The external game record must be linked to a game.",
                nameof(externalGameRecord)
            );


        if (externalGameRecord.GameId.Value != gameId)
            throw new ArgumentException
            (
                "The external game record must be linked to the same game.",
                nameof(externalGameRecord)
            );
        

        var normalizedExternalReleaseId = NormalizeExternalReleaseId(externalReleaseId);

        ArgumentNullException.ThrowIfNull(releaseDate);

        ValidateObservedAt(observedAt);

        return new GameRelease
        (
            Guid.NewGuid(),
            gameId,
            platformId,
            externalGameRecord.Id,
            normalizedExternalReleaseId,
            releaseDate,
            observedAt
        );
    }

    private static string NormalizeExternalReleaseId(string externalReleaseId)
    {
        if (string.IsNullOrWhiteSpace(externalReleaseId))
            throw new ArgumentException
            (
                "The external release identifier is required.",
                nameof(externalReleaseId)
            );
        

        var normalizedExternalReleaseId = externalReleaseId.Trim();

        if (normalizedExternalReleaseId.Length > MaximumExternalReleaseIdLength)
            throw new ArgumentException
            (
                $"The external release identifier cannot exceed " +
                $"{MaximumExternalReleaseIdLength} characters.",
                nameof(externalReleaseId)
            );
        

        return normalizedExternalReleaseId;
    }

    public void SetStatus(GameReleaseStatus? status)
    {
        if (status.HasValue && !Enum.IsDefined(status.Value))
            throw new ArgumentOutOfRangeException
            (
                nameof(status),
                status,
                "The release status is invalid."
            );
        
        Status = status;
    }

    public void SetRegionCode(string? regionCode)
    {
        if (string.IsNullOrWhiteSpace(regionCode))
        {
            RegionCode = null;
            return;
        }

        var normalizedRegionCode = regionCode.Trim().ToUpperInvariant();

        if (normalizedRegionCode.Length > MaximumRegionCodeLength)
        {
            throw new ArgumentException(
                $"The region code cannot exceed " +
                $"{MaximumRegionCodeLength} characters.",
                nameof(regionCode));
        }

        RegionCode = normalizedRegionCode;
    }

    public void SetEcosystem(string? ecosystem)
    {
        if (string.IsNullOrWhiteSpace(ecosystem))
        {
            Ecosystem = null;
            return;
        }

        var normalizedEcosystem = ecosystem.Trim();

        if (normalizedEcosystem.Length > MaximumEcosystemLength)

            throw new ArgumentException
            (
                $"The ecosystem cannot exceed " +
                $"{MaximumEcosystemLength} characters.",
                nameof(ecosystem)
            );
        

        Ecosystem = normalizedEcosystem;
    }

    public void UpdateReleaseDate(ReleaseDateValue releaseDate, DateTimeOffset observedAt)
    {
        ArgumentNullException.ThrowIfNull(releaseDate);

        ValidateObservedAt(observedAt);

        if (observedAt < ObservedAt)
            throw new ArgumentException
            (
                "The observation date cannot be earlier " +
                "than the current observation date.",
                nameof(observedAt)
            );
        

        ReleaseDate = releaseDate;
        ObservedAt = observedAt;
    }

    private static void ValidateRequiredId(Guid id, string parameterName)
    {
        if (id == Guid.Empty)

            throw new ArgumentException
            (
                "The identifier cannot be empty.",
                parameterName
            );
        
    }

    private static void ValidateObservedAt(DateTimeOffset observedAt)
    {
        if (observedAt == default)
            throw new ArgumentException
            (
                "The observation date must be provided.",
                nameof(observedAt)
            );
        
    }
}