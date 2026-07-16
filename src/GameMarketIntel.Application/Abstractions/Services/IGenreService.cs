using GameMarketIntel.Shared.Contracts.Genres;

namespace GameMarketIntel.Application.Abstractions.Services;

public interface IGenreService
{
    Task<IReadOnlyList<GenreDetails>> GetAllAsync(
        CancellationToken cancellationToken = default);
}