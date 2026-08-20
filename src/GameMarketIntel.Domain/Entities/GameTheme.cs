namespace GameMarketIntel.Domain.Entities;

public sealed class GameTheme
{
    public Guid GameId { get; private set; }
    public Guid ThemeId { get; private set; }
    public Guid ExternalGameRecordId { get; private set; }

    private GameTheme() { }

    public GameTheme(Guid gameId, Guid themeId, Guid externalGameRecordId)
    {
        if (gameId == Guid.Empty)
            throw new ArgumentException
            (
                "The Game Id is Required.", nameof(gameId)
            );
        
        if (themeId == Guid.Empty)
            throw new ArgumentException
            (
                "The Theme Id is Required.", nameof(themeId)
            );


        if (externalGameRecordId == Guid.Empty)
            throw new ArgumentException
            (
                "The External Id is Required.", nameof(externalGameRecordId)
            );
        
        GameId = gameId;
        ThemeId = themeId;
        ExternalGameRecordId = externalGameRecordId;
        
    }
}