using GameMarketIntel.Application.Abstractions.Persistence;
using GameMarketIntel.Application.Services;
using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Exceptions;
using NSubstitute;
using Shouldly;

namespace GameMarketIntel.Application.Tests.Services;

public sealed class GameServiceTest
{
    private readonly IGameRepository _gameRepository =
        Substitute.For<IGameRepository>();

    private readonly IGameImageRepository _gameImageRepository =
        Substitute.For<IGameImageRepository>();

    private readonly IGameImageUrlResolver _gameImageUrlResolver =
        Substitute.For<IGameImageUrlResolver>();

    private readonly GameService _service;

    public GameServiceTest()
    {
        _service = new GameService(
            _gameRepository,
            _gameImageRepository,
            _gameImageUrlResolver);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnGameDetails_WhenGameExists()
    {
        // Arrange
        var game = new Game(
            "Metroid Prime",
            "Description",
            new DateOnly(2002, 11, 18));

        _gameRepository
            .GetByIdAsync(
                game.Id,
                Arg.Any<CancellationToken>())
            .Returns(game);

        var primaryCover = new GameImageReference(
            "igdb",
            "co8abc",
            GameImageType.Cover);

        _gameImageRepository
            .GetPrimaryCoverAsync(
                game.Id,
                Arg.Any<CancellationToken>())
            .Returns(primaryCover);

        _gameImageUrlResolver
            .Resolve(
                primaryCover.DataSourceCode,
                primaryCover.SourceImageId,
                primaryCover.Type)
            .Returns(
                "https://images.igdb.com/igdb/image/upload/t_cover_big/co8abc.jpg");

        // Act
        var result = await _service.GetByIdAsync(game.Id);

        // Assert
        result.Id.ShouldBe(game.Id);
        result.Name.ShouldBe(game.Name);
        result.Description.ShouldBe(game.Description);
        result.ReleaseDate.ShouldBe(game.FirstReleaseDate);
        result.ImageUrl.ShouldBe(
            "https://images.igdb.com/igdb/image/upload/t_cover_big/co8abc.jpg");
        result.Genres.ShouldBeEmpty();
        result.Platforms.ShouldBeEmpty();

        await _gameImageRepository
            .Received(1)
            .GetPrimaryCoverAsync(
                game.Id,
                Arg.Any<CancellationToken>());

        _gameImageUrlResolver
            .Received(1)
            .Resolve(
                primaryCover.DataSourceCode,
                primaryCover.SourceImageId,
                primaryCover.Type);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNullImageUrl_WhenGameHasNoCover()
    {
        // Arrange
        var game = new Game("Metroid Prime");

        _gameRepository
            .GetByIdAsync(
                game.Id,
                Arg.Any<CancellationToken>())
            .Returns(game);

        _gameImageRepository
            .GetPrimaryCoverAsync(
                game.Id,
                Arg.Any<CancellationToken>())
            .Returns((GameImageReference?)null);

        // Act
        var result = await _service.GetByIdAsync(game.Id);

        // Assert
        result.ImageUrl.ShouldBeNull();

        _gameImageUrlResolver
            .DidNotReceiveWithAnyArgs()
            .Resolve(
                default!,
                default!,
                default);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenGameDoesNotExist()
    {
        // Arrange
        var gameId = Guid.NewGuid();

        _gameRepository
            .GetByIdAsync(
                gameId,
                Arg.Any<CancellationToken>())
            .Returns((Game?)null);

        // Act
        var action = () => _service.GetByIdAsync(gameId);

        // Assert
        await action.ShouldThrowAsync<NotFoundException>();

        await _gameImageRepository
            .DidNotReceiveWithAnyArgs()
            .GetPrimaryCoverAsync(
                default,
                default);
    }
}