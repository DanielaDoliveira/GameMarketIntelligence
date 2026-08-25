using GameMarketIntel.Domain.Enums;

namespace GameMarketIntel.Application.Abstractions.Persistence;

public interface IGameImageRepository
{
    Task<GameImageReference?> GetPrimaryCoverAsync(
        Guid gameId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, GameImageReference>> GetPrimaryCoversAsync(
        IReadOnlyCollection<Guid> gameIds,
        CancellationToken cancellationToken = default);
}

public sealed record GameImageReference(
    string DataSourceCode,
    string SourceImageId,
    GameImageType Type);