namespace GameMarketIntel.Domain.Entities;

public sealed class GameGameMode
{
    public Guid GameId { get; private set; }

    public Guid GameModeId { get; private set; }

    public Guid ExternalGameRecordId { get; private set; }

    private GameGameMode() { }

    public GameGameMode(Guid gameId, Guid gameModeId,ExternalGameRecord externalGameRecord)
    {
        if (gameId == Guid.Empty)
            throw new ArgumentException
            (
                "The game ID is required.",
                nameof(gameId)
            );


        if (gameModeId == Guid.Empty)
            throw new ArgumentException
            (
                "The game mode ID is required.",
                nameof(gameModeId)
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
        GameModeId = gameModeId;
        ExternalGameRecordId = externalGameRecord.Id;
    }
}