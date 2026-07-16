using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using GameMarketIntel.Infrastructure.Persistence.Repositories;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence.Repositories;

[Collection(PostgreSqlCollection.Name)]
public sealed class GameRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;

    public GameRepositoryTests(PostgreSqlFixture fixture)
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
    public async Task SearchAsync_ShouldReturnGamesMatchingPartialName()
    {
        // Arrange
        await using (var arrangeDbContext = _fixture.CreateDbContext())
        {
            arrangeDbContext.Games.AddRange(
                new Game(
                    name: "Hades",
                    description: null,
                    releaseDate: new DateOnly(2020, 9, 17),
                    imageUrl: null),
                new Game(
                    name: "Hades II",
                    description: null,
                    releaseDate: new DateOnly(2024, 5, 6),
                    imageUrl: null),
                new Game(
                    name: "Dead Cells",
                    description: null,
                    releaseDate: new DateOnly(2018, 8, 7),
                    imageUrl: null));

            await arrangeDbContext.SaveChangesAsync();
        }

        await using var queryDbContext = _fixture.CreateDbContext();

        var repository = new GameRepository(queryDbContext);

        // Act
        var result = await repository.SearchAsync(
            name: "hades",
            genreIds: [],
            platformIds: []);

        // Assert
        result.Count.ShouldBe(2);

        result
            .Select(game => game.Name)
            .ShouldBe(
                ["Hades II", "Hades"],
                ignoreOrder: false);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnGamesMatchingAnySelectedGenre()
    {
        // Arrange
        var action = new Genre("Action");
        var roguelike = new Genre("Roguelike");
        var platformGenre = new Genre("Platform");

        var pc = new Platform("PC");

        var hades = new Game(
            name: "Hades",
            releaseDate: new DateOnly(2020, 9, 17));

        hades.AddGenre(action);
        hades.AddGenre(roguelike);
        hades.AddPlatform(pc);

        var deadCells = new Game(
            name: "Dead Cells",
            releaseDate: new DateOnly(2018, 8, 7));

        deadCells.AddGenre(roguelike);
        deadCells.AddPlatform(pc);

        var celeste = new Game(
            name: "Celeste",
            releaseDate: new DateOnly(2018, 1, 25));

        celeste.AddGenre(platformGenre);
        celeste.AddPlatform(pc);

        await using (var arrangeDbContext = _fixture.CreateDbContext())
        {
            arrangeDbContext.Games.AddRange(
                hades,
                deadCells,
                celeste);

            await arrangeDbContext.SaveChangesAsync();
        }

        await using var queryDbContext = _fixture.CreateDbContext();

        var repository = new GameRepository(queryDbContext);

        // Act
        var result = await repository.SearchAsync(
            name: null,
            genreIds: [action.Id, roguelike.Id],
            platformIds: []);

        // Assert
        result.Count.ShouldBe(2);

        result
            .Select(game => game.Name)
            .ShouldBe(
                ["Hades", "Dead Cells"],
                ignoreOrder: true);
    }


    [Fact]
    public async Task SearchAsync_ShouldReturnGamesMatchingAnySelectedPlatform()
    {
        // Arrange
        var action = new Genre("Action");

        var pc = new Platform("PC");
        var switchPlatform = new Platform("Nintendo Switch");
        var playstation = new Platform("PlayStation 5");

        var hades = new Game( name: "Hades", releaseDate: new DateOnly(2020, 9, 17));

        hades.AddGenre(action);
        hades.AddPlatform(pc);

        var mario = new Game( name: "Super Mario Odyssey", releaseDate: new DateOnly(2017, 10, 27));

        mario.AddGenre(action);
        mario.AddPlatform(switchPlatform);

        var tlou = new Game( name: "The Last of Us", releaseDate: new DateOnly(2013, 6, 14));

        tlou.AddGenre(action);
        tlou.AddPlatform(playstation);

        await using (var arrangeDbContext = _fixture.CreateDbContext())
        {
            arrangeDbContext.Games.AddRange(
                hades,
                mario,
                tlou);

            await arrangeDbContext.SaveChangesAsync();
        }

        await using var queryDbContext = _fixture.CreateDbContext();

        var repository = new GameRepository(queryDbContext);

        // Act
        var result = await repository.SearchAsync(
            name: null,
            genreIds: [],
            platformIds: [pc.Id, switchPlatform.Id]);

        // Assert
        result.Count.ShouldBe(2);

        result
            .Select(game => game.Name)
            .ShouldBe(
                [
                    "Hades",
                "Super Mario Odyssey"
                ],
                ignoreOrder: true);
    }
    [Fact]
    public async Task SearchAsync_ShouldCombineGenreAndPlatformFiltersUsingAnd()
    {
        // Arrange
        var action = new Genre("Action");
        var rpg = new Genre("RPG");

        var pc = new Platform("PC");
        var switchPlatform = new Platform("Nintendo Switch");

        var hades = new Game(
            name: "Hades",
            releaseDate: new DateOnly(2020, 9, 17));

        hades.AddGenre(action);
        hades.AddPlatform(pc);

        var zelda = new Game(
            name: "The Legend of Zelda",
            releaseDate: new DateOnly(2017, 3, 3));

        zelda.AddGenre(action);
        zelda.AddPlatform(switchPlatform);

        var baldursGate = new Game(
            name: "Baldur's Gate 3",
            releaseDate: new DateOnly(2023, 8, 3));

        baldursGate.AddGenre(rpg);
        baldursGate.AddPlatform(pc);

        await using (var arrangeDbContext = _fixture.CreateDbContext())
        {
            arrangeDbContext.Games.AddRange(
                hades,
                zelda,
                baldursGate);

            await arrangeDbContext.SaveChangesAsync();
        }

        await using var queryDbContext = _fixture.CreateDbContext();

        var repository = new GameRepository(queryDbContext);

        // Act
        var result = await repository.SearchAsync(
            name: null,
            genreIds: [action.Id],
            platformIds: [pc.Id]);

        // Assert
        result.Count.ShouldBe(1);
        result.Single().Name.ShouldBe("Hades");
    }



    [Fact]
    public async Task SearchAsync_ShouldReturnEmptyWhenNoGameMatches()
    {
        // Arrange
        var action = new Genre("Action");
        var rpg = new Genre("RPG");

        var pc = new Platform("PC");

        var hades = new Game(
            name: "Hades",
            releaseDate: new DateOnly(2020, 9, 17));

        hades.AddGenre(action);
        hades.AddPlatform(pc);

        await using (var arrangeDbContext = _fixture.CreateDbContext())
        {
            arrangeDbContext.Games.Add(hades);

            await arrangeDbContext.SaveChangesAsync();
        }

        await using var queryDbContext = _fixture.CreateDbContext();

        var repository = new GameRepository(queryDbContext);

        // Act
        var result = await repository.SearchAsync(
            name: null,
            genreIds: [rpg.Id],
            platformIds: []);

        // Assert
        result.ShouldBeEmpty();
    }
}