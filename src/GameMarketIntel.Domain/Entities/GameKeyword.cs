namespace GameMarketIntel.Domain.Entities;

public sealed class GameKeyword
{
    public Guid GameId { get; private set; }

    public Guid KeywordId { get; private set; }

    public Guid ExternalGameRecordId { get; private set; }

    private GameKeyword() { }

    public GameKeyword(Guid gameId, Guid keywordId, Guid externalGameRecordId)
    {
        if (gameId == Guid.Empty)
            throw new ArgumentException
            (
                "The game ID is required.",
                nameof(gameId)
            );


        if (keywordId == Guid.Empty)
            throw new ArgumentException
            (
                "The keyword ID is required.",
                nameof(keywordId)
            );


        if (externalGameRecordId == Guid.Empty)
            throw new ArgumentException
            (
                "The external game record ID is required.",
                nameof(externalGameRecordId)
            );


        GameId = gameId;
        KeywordId = keywordId;
        ExternalGameRecordId = externalGameRecordId;
    }
}