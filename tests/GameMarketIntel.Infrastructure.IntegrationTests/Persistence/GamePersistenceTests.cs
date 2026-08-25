using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence;

[Collection(PostgreSqlCollection.Name)]
public sealed class GamePersistenceTests
{
    private readonly PostgreSqlFixture _fixture;

    public GamePersistenceTests(PostgreSqlFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task SaveAndLoad_ShouldPersistGameWithGenresAndPlatforms()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var game = new Game(
            name: "Hades",
            description: "A roguelike action game.",
            firstReleaseDate: new DateOnly(2020, 9, 17),
            productType: GameProductType.MainGame);

        var genre = new Genre("Action");

        var platform = new Platform(
            name: "Nintendo Switch",
            family: "Nintendo Switch",
            manufacturer: "Nintendo");

        game.AddGenre(genre);
        game.AddPlatform(platform);

        // Act
        await using (var writeDbContext = _fixture.CreateDbContext())
        {
            writeDbContext.Games.Add(game);

            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = _fixture.CreateDbContext();

        var persistedGame = await readDbContext.Games
            .Include(game => game.Genres)
            .Include(game => game.Platforms)
            .SingleAsync();

        // Assert
        persistedGame.Name.ShouldBe("Hades");
        persistedGame.NormalizedName.ShouldBe("HADES");
        persistedGame.Description.ShouldBe("A roguelike action game.");
        persistedGame.FirstReleaseDate.ShouldBe(new DateOnly(2020, 9, 17));
        persistedGame.ProductType.ShouldBe(GameProductType.MainGame);

        persistedGame.Genres.Count.ShouldBe(1);

        persistedGame.Genres
            .Single()
            .Name
            .ShouldBe("Action");

        persistedGame.Platforms.Count.ShouldBe(1);

        persistedGame.Platforms
            .Single()
            .Name
            .ShouldBe("Nintendo Switch");
    }

    [Fact]
    public async Task SaveChanges_ShouldRejectGenresWithTheSameNormalizedName()
    {
        // Arrange
        var firstGenre = new Genre("Strategy");
        var duplicateGenre = new Genre("  sTrAtEgY  ");

        await using var dbContext = _fixture.CreateDbContext();

        dbContext.Genres.AddRange(
            firstGenre,
            duplicateGenre);

        // Act
        var action = async () => await dbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveChanges_ShouldRejectPlatformsWithTheSameNormalizedName()
    {
        // Arrange
        var firstPlatform = new Platform("PlayStation 5");
        var duplicatePlatform = new Platform("  pLaYsTaTiOn 5  ");

        await using var dbContext = _fixture.CreateDbContext();

        dbContext.Platforms.AddRange(
            firstPlatform,
            duplicatePlatform);

        // Act
        var action = async () => await dbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }
}