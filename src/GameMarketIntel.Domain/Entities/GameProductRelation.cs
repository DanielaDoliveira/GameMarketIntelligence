using GameMarketIntel.Domain.Enums;

namespace GameMarketIntel.Domain.Entities;

public sealed class GameProductRelation
{
    public Guid Id { get; private set; }

    public Guid SourceGameId { get; private set; }

    public Guid TargetGameId { get; private set; }

    public Guid ExternalSourceGameRecordId { get; private set; }

    public Guid ExternalTargetGameRecordId { get; private set; }

    public GameProductRelationType RelationType { get; private set; }

    private GameProductRelation()
    {
    }

    public GameProductRelation(ExternalGameRecord sourceExternalGameRecord, ExternalGameRecord targetExternalGameRecord, GameProductRelationType relationType)
    {
        ArgumentNullException.ThrowIfNull(sourceExternalGameRecord);
        ArgumentNullException.ThrowIfNull(targetExternalGameRecord);

        if (sourceExternalGameRecord.GameId is null)
            throw new ArgumentException(
                "The source external game record must be linked to a canonical game.",
                nameof(sourceExternalGameRecord));

        if (targetExternalGameRecord.GameId is null)
            throw new ArgumentException(
                "The target external game record must be linked to a canonical game.",
                nameof(targetExternalGameRecord));

        if (sourceExternalGameRecord.DataSourceId != targetExternalGameRecord.DataSourceId)
            throw new ArgumentException(
                "Both external game records must belong to the same data source.",
                nameof(targetExternalGameRecord));

        if (sourceExternalGameRecord.GameId == targetExternalGameRecord.GameId)
            throw new ArgumentException(
                "A game cannot have a product relation with itself.",
                nameof(targetExternalGameRecord));

        Id = Guid.NewGuid();
        SourceGameId = sourceExternalGameRecord.GameId.Value;
        TargetGameId = targetExternalGameRecord.GameId.Value;
        ExternalSourceGameRecordId = sourceExternalGameRecord.Id;
        ExternalTargetGameRecordId = targetExternalGameRecord.Id;
        RelationType = relationType;
    }
}