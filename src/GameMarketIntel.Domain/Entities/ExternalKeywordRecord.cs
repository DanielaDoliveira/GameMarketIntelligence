namespace GameMarketIntel.Domain.Entities;

public sealed class ExternalKeywordRecord
{
    public const int MaxExternalIdLength = 100;

    public Guid Id { get; private set; }
    public Guid DataSourceId { get; private set; }
    public string ExternalId { get; private set; } = string.Empty;
    public Guid? KeywordId { get; private set; }
    public DateTimeOffset FirstSeenAt { get; private set; }
    public DateTimeOffset LastSeenAt { get; private set; }
    public DateTimeOffset? SourceUpdatedAt { get; private set; }

    private ExternalKeywordRecord() { }

    public ExternalKeywordRecord(Guid dataSourceId, string externalId, DateTimeOffset observedAt,
        DateTimeOffset? sourceUpdatedAt = null)
    {
        if (dataSourceId == Guid.Empty)
            throw new ArgumentException
            (
                "The data source ID is required.",
                nameof(dataSourceId)
            );


        ExternalId = NormalizeAndValidateExternalId(externalId);

        var normalizedObservedAt = NormalizeTimestamp
        (
            observedAt,
            nameof(observedAt)
        );

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

    public void LinkToKeyword(Guid keywordId)
    {
        if (keywordId == Guid.Empty)
            throw new ArgumentException
            (
                "The keyword ID is required.",
                nameof(keywordId)
            );


        if (KeywordId.HasValue)
        {
            if (KeywordId.Value == keywordId) return;


            throw new InvalidOperationException(
                "An external keyword record already linked to a keyword cannot be linked to a different keyword.");
        }

        KeywordId = keywordId;
    }

    public void Unlink()
    {
        if (KeywordId.HasValue)
            throw new InvalidOperationException("A linked external keyword record cannot be unlinked directly.");

        KeywordId = null;
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