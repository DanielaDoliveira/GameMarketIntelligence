using GameMarketIntel.Shared.Contracts.Genres;

namespace GameMarketIntel.Web.Services;

public interface IGenreApiService
{
    Task<IReadOnlyList<GenreDetails>> GetAllAsync(CancellationToken cancellationToken = default);
}