using GameMarketIntel.Application.Abstractions.Persistence;
using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameMarketIntel.Infrastructure.Persistence.Repositories;

public sealed class GameRepository( GameMarketIntelDbContext dbContext): IGameRepository
{
    public async Task<IReadOnlyList<Game>> SearchAsync(
        string? name,
        IReadOnlyCollection<Guid> genreIds,
        IReadOnlyCollection<Guid> platformIds,
        CancellationToken cancellationToken = default
        )
    {
        IQueryable<Game> query = dbContext.Games  .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var searchTerm = name.Trim();

            query = query.Where
                (
                game =>  EF.Functions.ILike
                    (
                        game.Name,
                        $"%{searchTerm}%"
                    )
                );
        }

        if (genreIds.Count > 0)
        {
            query = query.Where
                (
                    game =>
                    game.Genres.Any
                    (
                        genre =>
                        genreIds.Contains(genre.Id)
                     )
                 );
        }

        if (platformIds.Count > 0)
        {
            query = query.Where
                (
                    game =>
                    game.Platforms.Any
                    (
                        platform =>
                        platformIds.Contains(platform.Id)
                    )
                 );
        }

        return await query
            .Include(game => game.Genres)
            .Include(game => game.Platforms)
            .AsSplitQuery()
            .OrderBy(game => game.ReleaseDate == null)
            .ThenByDescending(game => game.ReleaseDate)
            .ThenBy(game => game.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Game?> GetByIdAsync( Guid id,CancellationToken cancellationToken = default)
    {
        return await dbContext.Games
            .AsNoTracking()
            .Include(game => game.Genres)
            .Include(game => game.Platforms)
            .AsSplitQuery()
            .SingleOrDefaultAsync
            (
                game => game.Id == id,
                cancellationToken
            );
    }
}