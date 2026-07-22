using GameMarketIntel.Domain.Entities;

namespace GameMarketIntel.Application.Abstractions.Persistence;

public interface IGameRepository
{
    Task<IReadOnlyList<Game>> SearchAsync(
        string? name,
        IReadOnlyCollection<Guid> genreIds,
        IReadOnlyCollection<Guid> platformIds,
        CancellationToken cancellationToken = default);

    Task<Game?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}