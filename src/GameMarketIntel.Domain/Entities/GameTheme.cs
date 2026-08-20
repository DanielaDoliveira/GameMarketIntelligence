namespace GameMarketIntel.Domain.Entities;

public sealed class GameTheme
{
    public Guid GameId { get; private set; }
    public Guid ThemeId { get; private set; }
    public Guid ExternalGameRecordId { get; private set; }

    private GameTheme() { }

    public GameTheme(Guid gameId, Guid themeId, ExternalGameRecord externalGameRecord)
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
        ThemeId = themeId;
        ExternalGameRecordId = externalGameRecord.Id;
    }
}
