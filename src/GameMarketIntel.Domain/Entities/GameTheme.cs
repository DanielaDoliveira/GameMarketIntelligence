namespace GameMarketIntel.Domain.Entities;
public sealed class GameTheme
{
    public Guid GameId { get; private set; }
    public Guid ThemeId { get; private set; }
    public Guid ExternalGameRecordId { get; private set; }
    public Guid ExternalThemeRecordId { get; private set; }

    private GameTheme() { }

    public GameTheme(Guid gameId, Guid themeId, ExternalGameRecord externalGameRecord, ExternalThemeRecord externalThemeRecord)
    {
        if (gameId == Guid.Empty)
            throw new ArgumentException
            (
                "The Game Id is Required.",
                nameof(gameId)
            );


        if (themeId == Guid.Empty)
            throw new ArgumentException
            (
                "The Theme Id is Required.",
                nameof(themeId)
            );
        

        ArgumentNullException.ThrowIfNull(externalGameRecord);
        ArgumentNullException.ThrowIfNull(externalThemeRecord);

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
        
        if (!externalThemeRecord.ThemeId.HasValue)

            throw new ArgumentException
            (
                "The External Theme Record must be linked to a Theme.",
                nameof(externalThemeRecord)
            );
        
        if (externalThemeRecord.ThemeId.Value != themeId)
            throw new ArgumentException
            (
                "The External Theme Record must be linked to the same Theme.",
                nameof(externalThemeRecord)
            );


        if (externalGameRecord.DataSourceId != externalThemeRecord.DataSourceId)
            throw new ArgumentException
            (
                "The External Game Record and External Theme Record must belong to the same Data Source.",
                nameof(externalThemeRecord)
            );
        

        GameId = gameId;
        ThemeId = themeId;
        ExternalGameRecordId = externalGameRecord.Id;
        ExternalThemeRecordId = externalThemeRecord.Id;
    }
}