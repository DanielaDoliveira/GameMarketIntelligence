using GameMarketIntel.Application.Games.Search;
using GameMarketIntel.Infrastructure.Persistence;
using GameMarketIntel.Shared.Contracts.Games.Search;
using Microsoft.EntityFrameworkCore;

namespace GameMarketIntel.Infrastructure.Games.Search;

public sealed class GameSearchRepository : IGameSearchRepository
{
    private readonly GameMarketIntelDbContext _dbContext;

    public GameSearchRepository(GameMarketIntelDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SearchGamesResult> SearchAsync(  SearchGamesQuery query,CancellationToken cancellationToken = default)
    {
        var gamesQuery = _dbContext.Games .AsNoTracking() .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchTerm = query.Search.Trim();
            gamesQuery = gamesQuery.Where(game =>  EF.Functions.ILike(game.Name, $"%{searchTerm}%"));
        }

        if (query.GenreId.HasValue)
        {
            gamesQuery = gamesQuery.Where(game =>
            game.Genres.Any(genre => genre.Id == query.GenreId.Value));
        }

        if (query.PlatformId.HasValue)
        {
            gamesQuery = gamesQuery.Where(game =>
                game.Platforms.Any(platform =>  platform.Id == query.PlatformId.Value));
        }

        if (query.ReleaseYear.HasValue)
        {
            gamesQuery = gamesQuery.Where(game =>game.ReleaseDate.HasValue &&  game.ReleaseDate.Value.Year == query.ReleaseYear.Value);
        }

        var totalItems = await gamesQuery.CountAsync(cancellationToken);

        var items = await gamesQuery
            .OrderBy(game => game.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(game => new GameSearchItem(
                game.Id,
                game.Name,
                game.Description,
                game.ReleaseDate,
                game.ImageUrl,
                game.Genres
                    .OrderBy(genre => genre.Name)
                    .Select(genre => new GameSearchCategory(
                        genre.Id,
                        genre.Name))
                    .ToArray(),
                game.Platforms
                    .OrderBy(platform => platform.Name)
                    .Select(platform => new GameSearchCategory(
                        platform.Id,
                        platform.Name))
                    .ToArray()))
            .ToArrayAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(
            totalItems / (double)query.PageSize);

        return new SearchGamesResult(
            items,
            query.Page,
            query.PageSize,
            totalItems,
            totalPages);
    }
}