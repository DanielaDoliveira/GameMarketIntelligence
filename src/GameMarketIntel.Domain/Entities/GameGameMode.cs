namespace GameMarketIntel.Domain.Entities;

public sealed class GameGameMode
{
    public Guid GameId { get; private set; }

    public Guid GameModeId { get; private set; }

    public Guid ExternalGameRecordId { get; private set; }

    private GameGameMode() { }

    public GameGameMode(Guid gameId, Guid gameModeId, Guid externalGameRecordId)
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


        if (externalGameRecordId == Guid.Empty)
            throw new ArgumentException
            (
                "The external game record ID is required.",
                nameof(externalGameRecordId)
            );


        GameId = gameId;
        GameModeId = gameModeId;
        ExternalGameRecordId = externalGameRecordId;
    }
}