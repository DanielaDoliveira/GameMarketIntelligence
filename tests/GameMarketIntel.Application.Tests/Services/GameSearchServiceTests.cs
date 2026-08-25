using GameMarketIntel.Application.Abstractions.Persistence;
using GameMarketIntel.Application.Games.Search;
using GameMarketIntel.Application.Services;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Shared.Contracts.Games.Search;
using NSubstitute;
using Shouldly;

namespace GameMarketIntel.Application.Tests.Services;

public sealed class GameSearchServiceTests
{
    private readonly IGameSearchRepository _repository =
        Substitute.For<IGameSearchRepository>();

    private readonly IGameImageRepository _gameImageRepository =
        Substitute.For<IGameImageRepository>();

    private readonly IGameImageUrlResolver _gameImageUrlResolver =
        Substitute.For<IGameImageUrlResolver>();

    private readonly SearchGamesQueryValidator _validator = new();

    [Fact]
    public async Task SearchAsync_ShouldEnrichItemsWithResolvedCoverUrl()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var query = new SearchGamesQuery(
            Search: null,
            GenreId: null,
            PlatformId: null,
            ReleaseYear: null);

        var item = new GameSearchItem(
            gameId,
            "Metroid Prime",
            "Description",
            new DateOnly(2002, 11, 18),
            null,
            Array.Empty<GameSearchCategory>(),
            Array.Empty<GameSearchCategory>());

        var repositoryResult = new SearchGamesResult(
            new[] { item },
            1,
            20,
            1,
            1);

        _repository
            .SearchAsync(
                query,
                Arg.Any<CancellationToken>())
            .Returns(repositoryResult);

        var primaryCover = new GameImageReference(
            "igdb",
            "co8abc",
            GameImageType.Cover);

        _gameImageRepository
            .GetPrimaryCoversAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids =>
                    ids.Count == 1 &&
                    ids.Contains(gameId)),
                Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, GameImageReference>
            {
                [gameId] = primaryCover
            });

        _gameImageUrlResolver
            .Resolve(
                primaryCover.DataSourceCode,
                primaryCover.SourceImageId,
                primaryCover.Type)
            .Returns(
                "https://images.igdb.com/igdb/image/upload/t_cover_big/co8abc.jpg");

        var service = new GameSearchService(
            _repository,
            _validator,
            _gameImageRepository,
            _gameImageUrlResolver);

        // Act
        var result = await service.SearchAsync(query);

        // Assert
        result.Items.Count.ShouldBe(1);
        result.Items.Single().ImageUrl.ShouldBe(
            "https://images.igdb.com/igdb/image/upload/t_cover_big/co8abc.jpg");
    }

    [Fact]
    public async Task SearchAsync_ShouldKeepImageUrlNull_WhenCoverDoesNotExist()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var query = new SearchGamesQuery(
            Search: null,
            GenreId: null,
            PlatformId: null,
            ReleaseYear: null);

        var item = new GameSearchItem(
            gameId,
            "Metroid Prime",
            null,
            null,
            null,
            Array.Empty<GameSearchCategory>(),
            Array.Empty<GameSearchCategory>());

        _repository
            .SearchAsync(
                query,
                Arg.Any<CancellationToken>())
            .Returns(new SearchGamesResult(
                new[] { item },
                1,
                20,
                1,
                1));

        _gameImageRepository
            .GetPrimaryCoversAsync(
                Arg.Any<IReadOnlyCollection<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, GameImageReference>());

        var service = new GameSearchService(
            _repository,
            _validator,
            _gameImageRepository,
            _gameImageUrlResolver);

        // Act
        var result = await service.SearchAsync(query);

        // Assert
        result.Items.Single().ImageUrl.ShouldBeNull();

        _gameImageUrlResolver
            .DidNotReceiveWithAnyArgs()
            .Resolve(
                default!,
                default!,
                default);
    }

    [Fact]
    public async Task SearchAsync_ShouldNotQueryImages_WhenResultIsEmpty()
    {
        // Arrange
        var query = new SearchGamesQuery(
            Search: null,
            GenreId: null,
            PlatformId: null,
            ReleaseYear: null);

        _repository
            .SearchAsync(
                query,
                Arg.Any<CancellationToken>())
            .Returns(new SearchGamesResult(
                Array.Empty<GameSearchItem>(),
                1,
                20,
                0,
                0));

        var service = new GameSearchService(
            _repository,
            _validator,
            _gameImageRepository,
            _gameImageUrlResolver);

        // Act
        var result = await service.SearchAsync(query);

        // Assert
        result.Items.ShouldBeEmpty();

        await _gameImageRepository
            .DidNotReceiveWithAnyArgs()
            .GetPrimaryCoversAsync(
                default!,
                default);
    }
}