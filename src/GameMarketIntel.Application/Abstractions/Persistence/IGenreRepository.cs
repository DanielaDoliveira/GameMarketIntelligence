using GameMarketIntel.Domain.Entities;

namespace GameMarketIntel.Application.Abstractions.Persistence;

public interface IGenreRepository
{
    Task<IReadOnlyList<Genre>> GetAllAsync(
        CancellationToken cancellationToken = default);
}