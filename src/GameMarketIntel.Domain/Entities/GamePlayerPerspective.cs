namespace GameMarketIntel.Domain.Entities;

public sealed class GamePlayerPerspective
{
    public Guid GameId { get; private set; }
    public Guid PlayerPerspectiveId { get; private set; }
    public Guid ExternalGameRecordId { get; private set; }
    public Guid ExternalPlayerPerspectiveRecordId { get; private set; }

    private GamePlayerPerspective() { }

    public GamePlayerPerspective(Guid gameId, Guid playerPerspectiveId, ExternalGameRecord externalGameRecord, ExternalPlayerPerspectiveRecord externalPlayerPerspectiveRecord)
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
        ArgumentNullException.ThrowIfNull(externalPlayerPerspectiveRecord);

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


        if (!externalPlayerPerspectiveRecord.PlayerPerspectiveId.HasValue)
            throw new ArgumentException
            (
                "The External Player Perspective Record must be linked to a Player Perspective.",
                nameof(externalPlayerPerspectiveRecord)
            );


        if (externalPlayerPerspectiveRecord.PlayerPerspectiveId.Value != playerPerspectiveId)
            throw new ArgumentException
            (
                "The External Player Perspective Record must be linked to the same Player Perspective.",
                nameof(externalPlayerPerspectiveRecord)
            );


        if (externalGameRecord.DataSourceId != externalPlayerPerspectiveRecord.DataSourceId)
            throw new ArgumentException
            (
                "The External Game Record and External Player Perspective Record must belong to the same Data Source.",
                nameof(externalPlayerPerspectiveRecord)
            );
        

        GameId = gameId;
        PlayerPerspectiveId = playerPerspectiveId;
        ExternalGameRecordId = externalGameRecord.Id;
        ExternalPlayerPerspectiveRecordId = externalPlayerPerspectiveRecord.Id;
    }
}