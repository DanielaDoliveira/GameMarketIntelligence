namespace GameMarketIntel.Domain.Entities;

public sealed class GameGameMode
{
    public Guid GameId { get; private set; }
    public Guid GameModeId { get; private set; }
    public Guid ExternalGameRecordId { get; private set; }
    public Guid ExternalGameModeRecordId { get; private set; }

    private GameGameMode() { }

    public GameGameMode(Guid gameId, Guid gameModeId, ExternalGameRecord externalGameRecord, ExternalGameModeRecord externalGameModeRecord)
    {
        if (gameId == Guid.Empty)
            throw new ArgumentException
            (
                "The Game Id is Required.",
                nameof(gameId)
            );


        if (gameModeId == Guid.Empty)
            throw new ArgumentException
            (
                "The Game Mode Id is Required.",
                nameof(gameModeId)
            );
        
        ArgumentNullException.ThrowIfNull(externalGameRecord);
        ArgumentNullException.ThrowIfNull(externalGameModeRecord);

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


        if (!externalGameModeRecord.GameModeId.HasValue)
            throw new ArgumentException
            (
                "The External Game Mode Record must be linked to a Game Mode.",
                nameof(externalGameModeRecord)
            );


        if (externalGameModeRecord.GameModeId.Value != gameModeId)
            throw new ArgumentException
            (
                "The External Game Mode Record must be linked to the same Game Mode.",
                nameof(externalGameModeRecord)
            );


        if (externalGameRecord.DataSourceId != externalGameModeRecord.DataSourceId)
            throw new ArgumentException
            (
                "The External Game Record and External Game Mode Record must belong to the same Data Source.",
                nameof(externalGameModeRecord)
            );
        

        GameId = gameId;
        GameModeId = gameModeId;
        ExternalGameRecordId = externalGameRecord.Id;
        ExternalGameModeRecordId = externalGameModeRecord.Id;
    }
}