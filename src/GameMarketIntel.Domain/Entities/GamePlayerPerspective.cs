namespace GameMarketIntel.Domain.Entities;

public sealed class GamePlayerPerspective
{
    public Guid GameId { get; private set; }

    public Guid PlayerPerspectiveId { get; private set; }

    public Guid ExternalGameRecordId { get; private set; }

    private GamePlayerPerspective() { }

    public GamePlayerPerspective(
        Guid gameId,
        Guid playerPerspectiveId,
        Guid externalGameRecordId)
    {
        if (gameId == Guid.Empty)
            throw new ArgumentException
            (
                "The game ID is required.",
                nameof(gameId)
            );


        if (playerPerspectiveId == Guid.Empty)
            throw new ArgumentException
            (
                "The player perspective ID is required.",
                nameof(playerPerspectiveId)
            );


        if (externalGameRecordId == Guid.Empty)
            throw new ArgumentException
            (
                "The external game record ID is required.",
                nameof(externalGameRecordId)
            );


        GameId = gameId;
        PlayerPerspectiveId = playerPerspectiveId;
        ExternalGameRecordId = externalGameRecordId;
    }
}