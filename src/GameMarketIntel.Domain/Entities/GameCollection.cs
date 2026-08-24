namespace GameMarketIntel.Domain.Entities;

public sealed class GameCollection
{
    public Guid GameId { get; private set; }

    public Guid CollectionId { get; private set; }

    public Guid ExternalGameRecordId { get; private set; }

    public Guid ExternalCollectionRecordId { get; private set; }

    private GameCollection()
    {
    }

    public GameCollection(
        ExternalGameRecord externalGameRecord,
        ExternalCollectionRecord externalCollectionRecord)
    {
        ArgumentNullException.ThrowIfNull(externalGameRecord);
        ArgumentNullException.ThrowIfNull(externalCollectionRecord);

        if (externalGameRecord.GameId is null)
            throw new ArgumentException(
                "The external game record must be linked to a canonical game.",
                nameof(externalGameRecord));

        if (externalCollectionRecord.CollectionId is null)
            throw new ArgumentException(
                "The external collection record must be linked to a canonical collection.",
                nameof(externalCollectionRecord));

        if (externalGameRecord.DataSourceId != externalCollectionRecord.DataSourceId)
            throw new ArgumentException(
                "The external game and collection records must belong to the same data source.",
                nameof(externalCollectionRecord));

        GameId = externalGameRecord.GameId.Value;
        CollectionId = externalCollectionRecord.CollectionId.Value;
        ExternalGameRecordId = externalGameRecord.Id;
        ExternalCollectionRecordId = externalCollectionRecord.Id;
    }
}