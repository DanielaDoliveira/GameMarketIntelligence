namespace GameMarketIntel.Domain.Entities;

public sealed class ExternalPlayerPerspectiveRecord
{
    public const int MaxExternalIdLength = 100;

    public Guid Id { get; private set; }
    public Guid DataSourceId { get; private set; }
    public string ExternalId { get; private set; } = string.Empty;
    public Guid? PlayerPerspectiveId { get; private set; }
    public DateTimeOffset FirstSeenAt { get; private set; }
    public DateTimeOffset LastSeenAt { get; private set; }
    public DateTimeOffset? SourceUpdatedAt { get; private set; }

    private ExternalPlayerPerspectiveRecord() { }

    public ExternalPlayerPerspectiveRecord(Guid dataSourceId, string externalId, DateTimeOffset observedAt, DateTimeOffset? sourceUpdatedAt = null)
    {
        if (dataSourceId == Guid.Empty)
        {
            throw new ArgumentException
            (
                "The data source ID is required.",
                nameof(dataSourceId)
            );
        }

        ExternalId = NormalizeAndValidateExternalId(externalId);

        var normalizedObservedAt = NormalizeTimestamp(observedAt, nameof(observedAt));

        Id = Guid.NewGuid();
        DataSourceId = dataSourceId;
        FirstSeenAt = normalizedObservedAt;
        LastSeenAt = normalizedObservedAt;
        SourceUpdatedAt = sourceUpdatedAt.HasValue
            ? NormalizeTimestamp
            (
                sourceUpdatedAt.Value,
                nameof(sourceUpdatedAt)
            )
            : null;
    }

    public void LinkToPlayerPerspective(Guid playerPerspectiveId)
    {
        if (playerPerspectiveId == Guid.Empty)
            throw new ArgumentException
            (
                "The player perspective ID is required.",
                nameof(playerPerspectiveId)
            );


        if (PlayerPerspectiveId.HasValue)
        {
            if (PlayerPerspectiveId.Value == playerPerspectiveId) return;

            throw new InvalidOperationException(
                "An external player perspective record already linked to a player perspective cannot be linked to a different player perspective.");
        }

        PlayerPerspectiveId = playerPerspectiveId;
    }

    public void Unlink()
    {
        if (PlayerPerspectiveId.HasValue)
            throw new InvalidOperationException("A linked external player perspective record cannot be unlinked directly.");


        PlayerPerspectiveId = null;
    }

    public void MarkSeen(DateTimeOffset observedAt, DateTimeOffset? sourceUpdatedAt = null)
    {
        var normalizedObservedAt = NormalizeTimestamp(observedAt, nameof(observedAt));

        if (normalizedObservedAt < LastSeenAt)
            throw new ArgumentException
            (
                "The observation timestamp cannot be earlier than the last observation.",
                nameof(observedAt)
            );


        LastSeenAt = normalizedObservedAt;

        if (!sourceUpdatedAt.HasValue) return;


        var normalizedSourceUpdatedAt = NormalizeTimestamp(sourceUpdatedAt.Value, nameof(sourceUpdatedAt));

        if (!SourceUpdatedAt.HasValue || normalizedSourceUpdatedAt > SourceUpdatedAt.Value)
            SourceUpdatedAt = normalizedSourceUpdatedAt;
    }

    private static string NormalizeAndValidateExternalId(string externalId)
    {
        if (string.IsNullOrWhiteSpace(externalId))
            throw new ArgumentException
            (
                "The external ID is required.",
                nameof(externalId)
            );


        var normalizedExternalId = externalId.Trim();

        if (normalizedExternalId.Length > MaxExternalIdLength)

            throw new ArgumentException
            (
                $"The external ID cannot exceed {MaxExternalIdLength} characters.",
                nameof(externalId)
            );


        return normalizedExternalId;
    }

    private static DateTimeOffset NormalizeTimestamp(DateTimeOffset timestamp, string parameterName)
    {
        if (timestamp == default)
        {
            throw new ArgumentException(
                "The timestamp is required.",
                parameterName);
        }

        return timestamp.ToUniversalTime();
    }
}