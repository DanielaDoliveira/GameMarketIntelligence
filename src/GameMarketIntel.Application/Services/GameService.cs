using GameMarketIntel.Application.Abstractions.Persistence;
using GameMarketIntel.Application.Abstractions.Services;
using GameMarketIntel.Exceptions;
using GameMarketIntel.Shared.Contracts.Games;
using GameMarketIntel.Shared.Contracts.Genres;
using GameMarketIntel.Shared.Contracts.Platforms;

namespace GameMarketIntel.Application.Services;

public sealed class GameService( IGameRepository gameRepository) : IGameService
{
    public async Task<GameDetails> GetByIdAsync( Guid id,CancellationToken cancellationToken = default)
    {
        var game = await gameRepository.GetByIdAsync( id,cancellationToken);

        if (game is null)
         throw new NotFoundException( $"Game '{id}' was not found.");
        

        return new GameDetails(
            game.Id,
            game.Name,
            game.Description,
            game.ReleaseDate,
            game.ImageUrl,
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