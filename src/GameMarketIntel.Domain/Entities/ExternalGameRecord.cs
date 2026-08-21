using GameMarketIntel.Domain.Enums;

namespace GameMarketIntel.Domain.Entities;

public sealed class ExternalGameRecord
{
    public const int MaxExternalIdLength = 100;

    public Guid Id { get; private set; }
    public Guid DataSourceId { get; private set; }
    public string ExternalId { get; private set; } = string.Empty;
    public Guid? GameId { get; private set; }
    public ExternalGameRecordStatus Status { get; private set; }
    public DateTimeOffset FirstSeenAt { get; private set; }
    public DateTimeOffset LastSeenAt { get; private set; }
    public DateTimeOffset? SourceUpdatedAt { get; private set; }
    
    private ExternalGameRecord() { }
    
    public ExternalGameRecord(Guid dataSourceId, string externalId, DateTimeOffset observedAt, DateTimeOffset? sourceUpdatedAt = null)
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
        Status = ExternalGameRecordStatus.Unlinked;
        FirstSeenAt = normalizedObservedAt;
        LastSeenAt = normalizedObservedAt;
        SourceUpdatedAt =
            sourceUpdatedAt.HasValue
                ? NormalizeTimestamp(sourceUpdatedAt.Value, nameof(sourceUpdatedAt))
                : null;
    }

    public void LinkToGame(Guid gameId)
    {
        if (gameId == Guid.Empty)
            throw new ArgumentException
            (
                "The game ID is required.",
                nameof(gameId)
                );
        

        if (Status == ExternalGameRecordStatus.Rejected)
            throw new InvalidOperationException("A rejected external game record cannot be linked directly.");
        

        if (Status == ExternalGameRecordStatus.Linked)
        {
            if (GameId == gameId) return;
            throw new InvalidOperationException("An external game record already linked to a game cannot be linked to a different game.");
        }

        GameId = gameId;
        Status = ExternalGameRecordStatus.Linked;
    }

    public void Unlink()
    {
        if (Status == ExternalGameRecordStatus.Linked)
            throw new InvalidOperationException("A linked external game record cannot be unlinked directly.");
        

        if (Status == ExternalGameRecordStatus.Rejected)
            throw new InvalidOperationException("A rejected external game record cannot be unlinked directly.");
        

        GameId = null;
        Status = ExternalGameRecordStatus.Unlinked;
    }
    public void Reject()
    {
        if (Status == ExternalGameRecordStatus.Linked)
            throw new InvalidOperationException("A linked external game record cannot be rejected directly.");
       
        if (Status == ExternalGameRecordStatus.Rejected) return;
        
        GameId = null;
        Status = ExternalGameRecordStatus.Rejected;
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