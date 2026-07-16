using GameMarketIntel.Application.Abstractions.Persistence;
using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameMarketIntel.Infrastructure.Persistence.Repositories;

public sealed class GenreRepository(GameMarketIntelDbContext dbContext): IGenreRepository
{
    public async Task<IReadOnlyList<Genre>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Genres
            .AsNoTracking()
            .OrderBy(genre => genre.Name)
            .ToListAsync(cancellationToken);
    }
}