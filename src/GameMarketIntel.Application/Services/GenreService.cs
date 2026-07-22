

using GameMarketIntel.Application.Abstractions.Persistence;
using GameMarketIntel.Application.Abstractions.Services;
using GameMarketIntel.Shared.Contracts.Genres;

namespace GameMarketIntel.Application.Services;

public sealed class GenreService ( IGenreRepository genreRepository) : IGenreService
{

    public async Task<IReadOnlyList<GenreDetails>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var genres = await genreRepository.GetAllAsync(cancellationToken);

        return genres.Select(genre => new GenreDetails(
                genre.Id,
                genre.Name
         )).ToList();
    }
}
