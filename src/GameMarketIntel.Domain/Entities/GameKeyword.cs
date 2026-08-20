namespace GameMarketIntel.Domain.Entities;

public sealed class GameKeyword
{
    public Guid GameId { get; private set; }
    public Guid KeywordId { get; private set; }
    public Guid ExternalGameRecordId { get; private set; }

    private GameKeyword() { }

    public GameKeyword(Guid gameId, Guid keywordId, ExternalGameRecord externalGameRecord)
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

        if (!externalGameRecord.GameId.HasValue)
            throw new ArgumentException
            (
                "The External Game Record must be linked to a Game.",
                nameof(externalGameRecord)
            );


        if (externalGameRecord.GameId.Value != gameId)
            throw new ArgumentException
            (
                "The External Game Record must be linked to the same Game.",
                nameof(externalGameRecord)
            );
        

        GameId = gameId;
        KeywordId = keywordId;
        ExternalGameRecordId = externalGameRecord.Id;
    }
}