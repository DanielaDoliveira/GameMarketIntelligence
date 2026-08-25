using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using GameMarketIntel.Infrastructure.Persistence.Repositories;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence.Repositories;

[Collection(PostgreSqlCollection.Name)]
public sealed class GameImageRepositoryTests
{
    private readonly PostgreSqlFixture _fixture;

    public GameImageRepositoryTests(PostgreSqlFixture fixture) =>
        _fixture = fixture;

    [Fact]
    public async Task GetPrimaryCoverAsync_ShouldReturnPrimaryCover()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var game = new Game("Metroid Prime");
        var dataSource = CreateDataSource("igdb");

        var externalGameRecord = CreateLinkedExternalGameRecord(
            dataSource,
            game);

        var screenshot = new GameImage(
            externalGameRecord,
            "screenshot-1",
            "sc-primary",
            GameImageType.Screenshot,
            sortOrder: 0);

        var laterCover = new GameImage(
            externalGameRecord,
            "cover-2",
            "co-later",
            GameImageType.Cover,
            sortOrder: 2);

        var primaryCover = new GameImage(
            externalGameRecord,
            "cover-1",
            "co-primary",
            GameImageType.Cover,
            sortOrder: 0);

        await SaveAsync(
            game,
            dataSource,
            externalGameRecord,
            screenshot,
            laterCover,
            primaryCover);

        await using var dbContext = _fixture.CreateDbContext();
        var repository = new GameImageRepository(dbContext);

        // Act
        var result = await repository.GetPrimaryCoverAsync(game.Id);

        // Assert
        result.ShouldNotBeNull();
        result.DataSourceCode.ShouldBe("igdb");
        result.SourceImageId.ShouldBe("co-primary");
        result.Type.ShouldBe(GameImageType.Cover);
    }

    [Fact]
    public async Task GetPrimaryCoverAsync_ShouldReturnNull_WhenGameHasNoCover()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var game = new Game("Metroid Prime");
        var dataSource = CreateDataSource("igdb");

        var externalGameRecord = CreateLinkedExternalGameRecord(
            dataSource,
            game);

        var screenshot = new GameImage(
            externalGameRecord,
            "screenshot-1",
            "sc-primary",
            GameImageType.Screenshot);

        await SaveAsync(
            game,
            dataSource,
            externalGameRecord,
            screenshot);

        await using var dbContext = _fixture.CreateDbContext();
        var repository = new GameImageRepository(dbContext);

        // Act
        var result = await repository.GetPrimaryCoverAsync(game.Id);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetPrimaryCoversAsync_ShouldReturnPrimaryCoverForEachGame()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var firstGame = new Game("Metroid Prime");
        var secondGame = new Game("Metroid Prime 2");
        var dataSource = CreateDataSource("igdb");

        var firstExternalGameRecord = CreateLinkedExternalGameRecord(
            dataSource,
            firstGame);

        var secondExternalGameRecord = CreateLinkedExternalGameRecord(
            dataSource,
            secondGame);

        var firstGameScreenshot = new GameImage(
            firstExternalGameRecord,
            "screenshot-1",
            "sc-first",
            GameImageType.Screenshot,
            sortOrder: 0);

        var firstGameLaterCover = new GameImage(
            firstExternalGameRecord,
            "cover-2",
            "co-first-later",
            GameImageType.Cover,
            sortOrder: 3);

        var firstGamePrimaryCover = new GameImage(
            firstExternalGameRecord,
            "cover-1",
            "co-first-primary",
            GameImageType.Cover,
            sortOrder: 0);

        var secondGameCover = new GameImage(
            secondExternalGameRecord,
            "cover-3",
            "co-second",
            GameImageType.Cover,
            sortOrder: 1);

        await SaveAsync(
            new[] { firstGame, secondGame },
            dataSource,
            new[] { firstExternalGameRecord, secondExternalGameRecord },
            firstGameScreenshot,
            firstGameLaterCover,
            firstGamePrimaryCover,
            secondGameCover);

        await using var dbContext = _fixture.CreateDbContext();
        var repository = new GameImageRepository(dbContext);

        // Act
        var result = await repository.GetPrimaryCoversAsync(
            new[] { firstGame.Id, secondGame.Id });

        // Assert
        result.Count.ShouldBe(2);

        result[firstGame.Id].DataSourceCode.ShouldBe("igdb");
        result[firstGame.Id].SourceImageId.ShouldBe("co-first-primary");
        result[firstGame.Id].Type.ShouldBe(GameImageType.Cover);

        result[secondGame.Id].DataSourceCode.ShouldBe("igdb");
        result[secondGame.Id].SourceImageId.ShouldBe("co-second");
        result[secondGame.Id].Type.ShouldBe(GameImageType.Cover);
    }

    private async Task SaveAsync(
        Game game,
        DataSource dataSource,
        ExternalGameRecord externalGameRecord,
        params GameImage[] images)
    {
        await SaveAsync(
            new[] { game },
            dataSource,
            new[] { externalGameRecord },
            images);
    }

    private async Task SaveAsync(
        IReadOnlyCollection<Game> games,
        DataSource dataSource,
        IReadOnlyCollection<ExternalGameRecord> externalGameRecords,
        params GameImage[] images)
    {
        await using var dbContext = _fixture.CreateDbContext();

        dbContext.Games.AddRange(games);
        dbContext.DataSources.Add(dataSource);
        dbContext.ExternalGameRecords.AddRange(externalGameRecords);
        dbContext.GameImages.AddRange(images);

        await dbContext.SaveChangesAsync();
    }

    private static ExternalGameRecord CreateLinkedExternalGameRecord(
        DataSource dataSource,
        Game game)
    {
        var externalGameRecord = new ExternalGameRecord(
            dataSource.Id,
            $"game-{Guid.NewGuid():N}",
            DateTimeOffset.UtcNow);

        externalGameRecord.LinkToGame(game.Id);

        return externalGameRecord;
    }

    private static DataSource CreateDataSource(string code)
    {
        var reliability = new SourceReliability(
            ReliabilityLevel.PublicDirect,
            "Dados obtidos diretamente de uma fonte pública.");

        return new DataSource(
            code,
            "IGDB",
            "https://www.igdb.com",
            reliability,
            attributionRequired: true);
    }
}