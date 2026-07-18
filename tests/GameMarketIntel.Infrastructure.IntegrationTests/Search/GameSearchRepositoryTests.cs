using GameMarketIntel.Application.Games.Search;
using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Infrastructure.Games.Search;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Games.Search;

[Collection(PostgreSqlCollection.Name)]
public sealed class GameSearchRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;

    public GameSearchRepositoryTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync()
    {
        return _fixture.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnRequestedPageWithPaginationMetadata()
    {
        // Arrange
        await using (var arrangeDbContext = _fixture.CreateDbContext())
        {
            arrangeDbContext.Games.AddRange(
                new Game(
                    name: "Celeste",
                    releaseDate: new DateOnly(2018, 1, 25)),
                new Game(
                    name: "Hades",
                    releaseDate: new DateOnly(2020, 9, 17)),
                new Game(
                    name: "Inside",
                    releaseDate: new DateOnly(2016, 6, 29)));

            await arrangeDbContext.SaveChangesAsync();
        }

        await using var queryDbContext = _fixture.CreateDbContext();

        var repository = new GameSearchRepository(queryDbContext);

        var query = new SearchGamesQuery(
            Search: null,
            GenreId: null,
            PlatformId: null,
            ReleaseYear: null,
            Page: 2,
            PageSize: 2);

        // Act
        var result = await repository.SearchAsync(query);

        // Assert
        result.Page.ShouldBe(2);
        result.PageSize.ShouldBe(2);
        result.TotalItems.ShouldBe(3);
        result.TotalPages.ShouldBe(2);

        result.Items.Count.ShouldBe(1);
        result.Items.Single().Name.ShouldBe("Inside");
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterByPartialNameIgnoringCase()
    {
        // Arrange
        await using (var arrangeDbContext = _fixture.CreateDbContext())
        {
            arrangeDbContext.Games.AddRange(
                new Game(
                    name: "The Legend of Zelda",
                    releaseDate: new DateOnly(1986, 2, 21)),
                new Game(
                    name: "Zelda II: The Adventure of Link",
                    releaseDate: new DateOnly(1987, 1, 14)),
                new Game(
                    name: "Hades",
                    releaseDate: new DateOnly(2020, 9, 17)));

            await arrangeDbContext.SaveChangesAsync();
        }

        await using var queryDbContext = _fixture.CreateDbContext();

        var repository = new GameSearchRepository(queryDbContext);

        var query = new SearchGamesQuery(
            Search: "zELdA",
            GenreId: null,
            PlatformId: null,
            ReleaseYear: null,
            Page: 1,
            PageSize: 20);

        // Act
        var result = await repository.SearchAsync(query);

        // Assert
        result.TotalItems.ShouldBe(2);
        result.TotalPages.ShouldBe(1);
        result.Items.Count.ShouldBe(2);

        result.Items
            .Select(item => item.Name)
            .ShouldBe(
            [
                "The Legend of Zelda",
            "Zelda II: The Adventure of Link"
            ]);
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterByGenre()
    {
        // Arrange
        Guid actionGenreId;

        await using (var arrangeDbContext = _fixture.CreateDbContext())
        {
            var action = new Genre("Action");
            var puzzle = new Genre("Puzzle");

            var hades = new Game(
                name: "Hades",
                releaseDate: new DateOnly(2020, 9, 17));

            var portal = new Game(
                name: "Portal",
                releaseDate: new DateOnly(2007, 10, 10));

            hades.AddGenre(action);
            portal.AddGenre(puzzle);

            arrangeDbContext.Games.AddRange(hades, portal);

            await arrangeDbContext.SaveChangesAsync();

            actionGenreId = action.Id;
        }

        await using var queryDbContext = _fixture.CreateDbContext();

        var repository = new GameSearchRepository(queryDbContext);

        var query = new SearchGamesQuery(
            Search: null,
            GenreId: actionGenreId,
            PlatformId: null,
            ReleaseYear: null,
            Page: 1,
            PageSize: 20);

        // Act
        var result = await repository.SearchAsync(query);

        // Assert
        result.TotalItems.ShouldBe(1);
        result.Items.Count.ShouldBe(1);
        result.Items.Single().Name.ShouldBe("Hades");

        result.Items.Single().Genres
            .Select(genre => genre.Name)
            .ShouldContain("Action");
    }
    [Fact]
    public async Task SearchAsync_ShouldFilterByPlatform()
    {
        // Arrange
        Guid switchPlatformId;

        await using (var arrangeDbContext = _fixture.CreateDbContext())
        {
            var nintendoSwitch = new Platform(
                name: "Nintendo Switch");

            var playStation5 = new Platform(
                name: "PlayStation 5");

            var zelda = new Game(
                name: "The Legend of Zelda: Breath of the Wild",
                releaseDate: new DateOnly(2017, 3, 3));

            var returnal = new Game(
                name: "Returnal",
                releaseDate: new DateOnly(2021, 4, 30));

            zelda.AddPlatform(nintendoSwitch);
            returnal.AddPlatform(playStation5);

            arrangeDbContext.Games.AddRange(zelda, returnal);

            await arrangeDbContext.SaveChangesAsync();

            switchPlatformId = nintendoSwitch.Id;
        }

        await using var queryDbContext = _fixture.CreateDbContext();

        var repository = new GameSearchRepository(queryDbContext);

        var query = new SearchGamesQuery(
            Search: null,
            GenreId: null,
            PlatformId: switchPlatformId,
            ReleaseYear: null,
            Page: 1,
            PageSize: 20);

        // Act
        var result = await repository.SearchAsync(query);

        // Assert
        result.TotalItems.ShouldBe(1);
        result.Items.Count.ShouldBe(1);
        result.Items.Single().Name.ShouldBe(
            "The Legend of Zelda: Breath of the Wild");

        result.Items.Single().Platforms
            .Select(platform => platform.Name)
            .ShouldContain("Nintendo Switch");
    }


    [Fact]
    public async Task SearchAsync_ShouldFilterByReleaseYear()
    {
        // Arrange
        await using (var arrangeDbContext = _fixture.CreateDbContext())
        {
            arrangeDbContext.Games.AddRange(
                new Game(
                    name: "Hades",
                    releaseDate: new DateOnly(2020, 9, 17)),
                new Game(
                    name: "The Last of Us Part II",
                    releaseDate: new DateOnly(2020, 6, 19)),
                new Game(
                    name: "Returnal",
                    releaseDate: new DateOnly(2021, 4, 30)),
                new Game(
                    name: "Unreleased Game",
                    releaseDate: null));

            await arrangeDbContext.SaveChangesAsync();
        }

        await using var queryDbContext = _fixture.CreateDbContext();

        var repository = new GameSearchRepository(queryDbContext);

        var query = new SearchGamesQuery(
            Search: null,
            GenreId: null,
            PlatformId: null,
            ReleaseYear: 2020,
            Page: 1,
            PageSize: 20);

        // Act
        var result = await repository.SearchAsync(query);

        // Assert
        result.TotalItems.ShouldBe(2);
        result.Items.Count.ShouldBe(2);

        result.Items
            .Select(item => item.Name)
            .ShouldBe(
            [
                "Hades",
            "The Last of Us Part II"
            ]);

        result.Items
            .ShouldAllBe(item =>
                item.ReleaseDate.HasValue &&
                item.ReleaseDate.Value.Year == 2020);
    }

    [Fact]
    public async Task SearchAsync_ShouldCombineFilters()
    {
        // Arrange
        Guid actionGenreId;
        Guid pcPlatformId;

        await using (var arrangeDbContext = _fixture.CreateDbContext())
        {
            var action = new Genre("Action");
            var puzzle = new Genre("Puzzle");

            var pc = new Platform("PC");
            var nintendoSwitch = new Platform("Nintendo Switch");

            var hades = new Game(
                name: "Hades",
                releaseDate: new DateOnly(2020, 9, 17));

            hades.AddGenre(action);
            hades.AddPlatform(pc);

            var celeste = new Game(
                name: "Celeste",
                releaseDate: new DateOnly(2018, 1, 25));

            celeste.AddGenre(action);
            celeste.AddPlatform(nintendoSwitch);

            var portal = new Game(
                name: "Portal",
                releaseDate: new DateOnly(2007, 10, 10));

            portal.AddGenre(puzzle);
            portal.AddPlatform(pc);

            arrangeDbContext.Games.AddRange(
                hades,
                celeste,
                portal);

            await arrangeDbContext.SaveChangesAsync();

            actionGenreId = action.Id;
            pcPlatformId = pc.Id;
        }

        await using var queryDbContext = _fixture.CreateDbContext();

        var repository = new GameSearchRepository(queryDbContext);

        var query = new SearchGamesQuery(
            Search: "had",
            GenreId: actionGenreId,
            PlatformId: pcPlatformId,
            ReleaseYear: 2020,
            Page: 1,
            PageSize: 20);

        // Act
        var result = await repository.SearchAsync(query);

        // Assert
        result.TotalItems.ShouldBe(1);
        result.Items.Count.ShouldBe(1);
        result.Items.Single().Name.ShouldBe("Hades");
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnEmptyResultWhenNoGamesMatch()
    {
        // Arrange
        await using (var arrangeDbContext = _fixture.CreateDbContext())
        {
            arrangeDbContext.Games.AddRange(
                new Game(
                    name: "Hades",
                    releaseDate: new DateOnly(2020, 9, 17)),
                new Game(
                    name: "Celeste",
                    releaseDate: new DateOnly(2018, 1, 25)));

            await arrangeDbContext.SaveChangesAsync();
        }

        await using var queryDbContext = _fixture.CreateDbContext();

        var repository = new GameSearchRepository(queryDbContext);

        var query = new SearchGamesQuery(
            Search: "Zelda",
            GenreId: null,
            PlatformId: null,
            ReleaseYear: null,
            Page: 1,
            PageSize: 20);

        // Act
        var result = await repository.SearchAsync(query);

        // Assert
        result.Items.ShouldBeEmpty();
        result.TotalItems.ShouldBe(0);
        result.TotalPages.ShouldBe(0);
        result.Page.ShouldBe(1);
        result.PageSize.ShouldBe(20);
    }
}
