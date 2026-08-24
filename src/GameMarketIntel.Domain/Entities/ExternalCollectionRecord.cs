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

    public ExternalCollectionRecord(Guid dataSourceId, string externalId, DateTimeOffset observedAt)
    {
        if (dataSourceId == Guid.Empty)
            throw new ArgumentException(
                "The data source ID cannot be empty.",
                nameof(dataSourceId));

        if (observedAt == default)
            throw new ArgumentException(
                "The observed timestamp cannot be the default value.",
                nameof(observedAt));

        Id = Guid.NewGuid();
        DataSourceId = dataSourceId;
        ExternalId = NormalizeAndValidateExternalId(externalId);
        FirstSeenAt = observedAt;
        LastSeenAt = observedAt;
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

    public void MarkSeen(DateTimeOffset observedAt, DateTimeOffset? sourceUpdatedAt = null)
    {
        if (observedAt == default)
            throw new ArgumentException(
                "The observed timestamp cannot be the default value.",
                nameof(observedAt));

        if (observedAt > LastSeenAt)
            LastSeenAt = observedAt;

        if (sourceUpdatedAt.HasValue &&
            (!SourceUpdatedAt.HasValue || sourceUpdatedAt.Value > SourceUpdatedAt.Value))
            SourceUpdatedAt = sourceUpdatedAt;
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
}