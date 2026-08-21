namespace GameMarketIntel.Domain.Entities;

public sealed class GameKeyword
{
    public Guid GameId { get; private set; }
    public Guid KeywordId { get; private set; }
    public Guid ExternalGameRecordId { get; private set; }
    public Guid ExternalKeywordRecordId { get; private set; }

    private GameKeyword() { }

    public GameKeyword(Guid gameId, Guid keywordId, ExternalGameRecord externalGameRecord, ExternalKeywordRecord externalKeywordRecord)
    {
        if (gameId == Guid.Empty)
            throw new ArgumentException
            (
                "The Game Id is Required.",
                nameof(gameId)
            );
        

        if (keywordId == Guid.Empty)
            throw new ArgumentException
            (
                "The Keyword Id is Required.",
                nameof(keywordId)
            );
        

        ArgumentNullException.ThrowIfNull(externalGameRecord);
        ArgumentNullException.ThrowIfNull(externalKeywordRecord);

        if (!externalGameRecord.GameId.HasValue)
            throw new ArgumentException
            (
                "The External Game Record must be linked to a Game.",
                nameof(externalGameRecord)
            );
        

        if (externalGameRecord.GameId.Value != gameId)
            throw new ArgumentException(
                "The External Game Record must be linked to the same Game.",
                nameof(externalGameRecord));
        
        
        if (!externalKeywordRecord.KeywordId.HasValue)
            throw new ArgumentException
            (
                "The External Keyword Record must be linked to a Keyword.",
                nameof(externalKeywordRecord)
            );
        

        if (externalKeywordRecord.KeywordId.Value != keywordId)
            throw new ArgumentException
            (
                "The External Keyword Record must be linked to the same Keyword.",
                nameof(externalKeywordRecord)
            );
        

        if (externalGameRecord.DataSourceId != externalKeywordRecord.DataSourceId)
            throw new ArgumentException
            (
                "The External Game Record and External Keyword Record must belong to the same Data Source.",
                nameof(externalKeywordRecord)
            );
        

        GameId = gameId;
        KeywordId = keywordId;
        ExternalGameRecordId = externalGameRecord.Id;
        ExternalKeywordRecordId = externalKeywordRecord.Id;
    }
}