namespace GameMarketIntel.Domain.Entities;

public sealed class ExternalGameModeRecord
{
    public const int MaxExternalIdLength = 100;

    public Guid Id { get; private set; }
    public Guid DataSourceId { get; private set; }
    public string ExternalId { get; private set; } = string.Empty;
    public Guid? GameModeId { get; private set; }
    public DateTimeOffset FirstSeenAt { get; private set; }
    public DateTimeOffset LastSeenAt { get; private set; }
    public DateTimeOffset? SourceUpdatedAt { get; private set; }

    private ExternalGameModeRecord() { }

    public ExternalGameModeRecord(Guid dataSourceId, string externalId, DateTimeOffset observedAt, DateTimeOffset? sourceUpdatedAt = null)
    {
        if (dataSourceId == Guid.Empty)
            throw new ArgumentException
            (
                "The data source ID is required.",
                nameof(dataSourceId)
            );
        
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

    public void LinkToGameMode(Guid gameModeId)
    {
        if (gameModeId == Guid.Empty)
            throw new ArgumentException
            (
                "The game mode ID is required.",
                nameof(gameModeId)
            );


        if (GameModeId.HasValue)
        {
            if (GameModeId.Value == gameModeId) return;

            throw new InvalidOperationException("An external game mode record already linked to a game mode cannot be linked to a different game mode.");
        }

        GameModeId = gameModeId;
    }

    public void Unlink()
    {
        if (GameModeId.HasValue)
            throw new InvalidOperationException("A linked external game mode record cannot be unlinked directly.");

        GameModeId = null;
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
            throw new ArgumentException("The timestamp is required.", parameterName);

        return timestamp.ToUniversalTime();
    }
}