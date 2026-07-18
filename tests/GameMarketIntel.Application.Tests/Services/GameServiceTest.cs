using GameMarketIntel.Application.Abstractions.Persistence;
using GameMarketIntel.Application.Services;
using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Exceptions;
using NSubstitute;
using Shouldly;

namespace GameMarketIntel.Application.Tests.Services;

public sealed class GameServiceTests
{
    [Fact]
    public async Task GetByIdAsync_ShouldReturnMappedGameDetails_WhenGameExists()
    {
        // Arrange
        var genre = new Genre("Action");

        var platform = new Platform(
            "PlayStation 5",
            family: "PlayStation",
            manufacturer: "Sony",
            imageUrl: "https://example.com/playstation-5.png");

        var game = new Game(
            "Astro Bot",
            description: "A platform adventure game.",
            releaseDate: new DateOnly(2024, 9, 6),
            imageUrl: "https://example.com/astro-bot.png");

        game.AddGenre(genre);
        game.AddPlatform(platform);

        var repository = Substitute.For<IGameRepository>();

        repository
            .GetByIdAsync(
                game.Id,
                Arg.Any<CancellationToken>())
            .Returns(game);

        var service = new GameService(repository);

        // Act
        var result = await service.GetByIdAsync(game.Id);

        // Assert
        result.Id.ShouldBe(game.Id);
        result.Name.ShouldBe("Astro Bot");
        result.Description.ShouldBe(
            "A platform adventure game.");
        result.ReleaseDate.ShouldBe(
            new DateOnly(2024, 9, 6));
        result.ImageUrl.ShouldBe(
            "https://example.com/astro-bot.png");

        result.Genres.Count.ShouldBe(1);
        result.Genres[0].Id.ShouldBe(genre.Id);
        result.Genres[0].Name.ShouldBe("Action");

        result.Platforms.Count.ShouldBe(1);
        result.Platforms[0].Id.ShouldBe(platform.Id);
        result.Platforms[0].Name.ShouldBe("PlayStation 5");
        result.Platforms[0].Family.ShouldBe("PlayStation");
        result.Platforms[0].Manufacturer.ShouldBe("Sony");
        result.Platforms[0].ImageUrl.ShouldBe(
            "https://example.com/playstation-5.png");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenGameDoesNotExist()
    {
        // Arrange
        var gameId = Guid.NewGuid();

        var repository = Substitute.For<IGameRepository>();

        repository .GetByIdAsync(gameId,Arg.Any<CancellationToken>()) .Returns((Game?)null);

        var service = new GameService(repository);

        // Act
        var exception = await Should.ThrowAsync<NotFoundException>(() => service.GetByIdAsync(gameId));

        // Assert
        exception.Message.ShouldBe( $"Game '{gameId}' was not found.");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldPassCancellationTokenToRepository()
    {
        // Arrange
        var game = new Game("Celeste");

        var repository = Substitute.For<IGameRepository>();

        repository
            .GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);

        var service = new GameService(repository);

        using var cancellationTokenSource =new CancellationTokenSource();

        var cancellationToken =cancellationTokenSource.Token;

        // Act
        await service.GetByIdAsync( game.Id,cancellationToken);

        // Assert
        await repository.Received(1).GetByIdAsync(game.Id, cancellationToken);
    }
}