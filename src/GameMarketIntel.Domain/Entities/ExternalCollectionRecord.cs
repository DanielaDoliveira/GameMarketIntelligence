namespace GameMarketIntel.Domain.Entities;

public sealed class ExternalCollectionRecord
{
    public const int MaxExternalIdLength = 100;

    public Guid Id { get; private set; }

    public Guid DataSourceId { get; private set; }

    public string ExternalId { get; private set; } = string.Empty;

    public Guid? CollectionId { get; private set; }

    public DateTimeOffset FirstSeenAt { get; private set; }

    public DateTimeOffset LastSeenAt { get; private set; }

    public DateTimeOffset? SourceUpdatedAt { get; private set; }

    private ExternalCollectionRecord()
    {
    }

    public ExternalCollectionRecord(
        Guid dataSourceId, 
        string externalId, 
        DateTimeOffset observedAt,  
        DateTimeOffset? sourceUpdatedAt = null)
    {
        if (dataSourceId == Guid.Empty)
            throw new ArgumentException(
                "The data source ID cannot be empty.",
                nameof(dataSourceId));
        
        var normalizedObservedAt = NormalizeTimestamp(observedAt, nameof(observedAt));
        
        Id = Guid.NewGuid();
        DataSourceId = dataSourceId;
        ExternalId = NormalizeAndValidateExternalId(externalId);
        FirstSeenAt = normalizedObservedAt;
        LastSeenAt = normalizedObservedAt;
        SourceUpdatedAt = sourceUpdatedAt.HasValue
            ? NormalizeTimestamp(sourceUpdatedAt.Value, nameof(sourceUpdatedAt))
            : null;
    }

    public void LinkToCollection(Guid collectionId)
    {
        if (collectionId == Guid.Empty)
            throw new ArgumentException(
                "The collection ID cannot be empty.",
                nameof(collectionId));

        if (CollectionId.HasValue && CollectionId.Value != collectionId)
            throw new InvalidOperationException(
                "The external collection record is already linked to another collection.");

        CollectionId = collectionId;
    }

    public void MarkSeen(
        DateTimeOffset observedAt,
        DateTimeOffset? sourceUpdatedAt = null)
    {
        var normalizedObservedAt = NormalizeTimestamp(observedAt, nameof(observedAt));

        if (normalizedObservedAt < LastSeenAt)
            throw new ArgumentException(
                "The observation timestamp cannot be earlier than the last observation.",
                nameof(observedAt));

        LastSeenAt = normalizedObservedAt;

        if (!sourceUpdatedAt.HasValue)
            return;

        var normalizedSourceUpdatedAt = NormalizeTimestamp(sourceUpdatedAt.Value, nameof(sourceUpdatedAt));

        if (!SourceUpdatedAt.HasValue || normalizedSourceUpdatedAt > SourceUpdatedAt.Value)
            SourceUpdatedAt = normalizedSourceUpdatedAt;
    }

    private static string NormalizeAndValidateExternalId(string externalId)
    {
        if (string.IsNullOrWhiteSpace(externalId))
            throw new ArgumentException(
                "The external ID cannot be null, empty or whitespace.",
                nameof(externalId));

        var normalizedExternalId = externalId.Trim();

        if (normalizedExternalId.Length > MaxExternalIdLength)
            throw new ArgumentException(
                $"The external ID cannot exceed {MaxExternalIdLength} characters.",
                nameof(externalId));

        return normalizedExternalId;
    }
    
    private static DateTimeOffset NormalizeTimestamp(DateTimeOffset timestamp, string parameterName)
    {
        if (timestamp == default)
            throw new ArgumentException(
                "The timestamp cannot be the default value.",
                parameterName);

        return timestamp.ToUniversalTime();
    }
}