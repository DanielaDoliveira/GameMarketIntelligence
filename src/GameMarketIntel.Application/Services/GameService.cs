
using GameMarketIntel.Application.Abstractions.Persistence;
using GameMarketIntel.Application.Abstractions.Services;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Exceptions;
using GameMarketIntel.Shared.Contracts.Games;
using GameMarketIntel.Shared.Contracts.Genres;
using GameMarketIntel.Shared.Contracts.Platforms;

namespace GameMarketIntel.Application.Services;

public sealed class GameService(
    IGameRepository gameRepository,
    IGameImageRepository gameImageRepository,
    IGameImageUrlResolver gameImageUrlResolver) : IGameService
{
    public async Task<GameDetails> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var game = await gameRepository.GetByIdAsync(id, cancellationToken);

        if (game is null)
            throw new NotFoundException($"Game '{id}' was not found.");

        var primaryCover = await gameImageRepository.GetPrimaryCoverAsync(
            game.Id,
            cancellationToken);

        var imageUrl = primaryCover is null
            ? null
            : gameImageUrlResolver.Resolve(
                primaryCover.DataSourceCode,
                primaryCover.SourceImageId,
                primaryCover.Type);

        return new GameDetails(
            game.Id,
            game.Name,
            game.Description,
            game.FirstReleaseDate,
            imageUrl,
            game.Genres
                .Select(genre => new GenreDetails(
                    genre.Id,
                    genre.Name))
                .ToList(),
            game.Platforms
                .Select(platform => new PlatformDetails(
                    platform.Id,
                    platform.Name,
                    platform.Family,
                    platform.Manufacturer,
                    platform.ImageUrl))
                .ToList());
    }
}