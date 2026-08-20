namespace GameMarketIntel.Domain.Entities;

public sealed class GamePlayerPerspective
{
    public Guid GameId { get; private set; }
    public Guid PlayerPerspectiveId { get; private set; }
    public Guid ExternalGameRecordId { get; private set; }

    private GamePlayerPerspective() { }

    public GamePlayerPerspective(Guid gameId, Guid playerPerspectiveId, ExternalGameRecord externalGameRecord)
    {
        if (gameId == Guid.Empty)
            throw new ArgumentException
            (
                "The Game Id is Required.",
                nameof(gameId)
            );


        if (playerPerspectiveId == Guid.Empty)
            throw new ArgumentException
            (
                "The Player Perspective Id is Required.",
                nameof(playerPerspectiveId)
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
        PlayerPerspectiveId = playerPerspectiveId;
        ExternalGameRecordId = externalGameRecord.Id;
    }
}