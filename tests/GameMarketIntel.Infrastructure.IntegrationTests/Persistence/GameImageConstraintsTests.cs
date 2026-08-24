using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence;

[Collection(PostgreSqlCollection.Name)]
public sealed class GameImageConstraintTests
{
    private readonly PostgreSqlFixture _fixture;

    public GameImageConstraintTests(PostgreSqlFixture fixture) =>
        _fixture = fixture;

    [Fact]
    public async Task SaveChanges_ShouldRejectDuplicateExternalImageIdentity()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var game = new Game("Metroid Prime");
        var dataSource = CreateDataSource();

        var externalGameRecord = new ExternalGameRecord(
            dataSource.Id,
            "1234",
            DateTimeOffset.UtcNow);

        externalGameRecord.LinkToGame(game.Id);

        var firstImage = new GameImage(
            externalGameRecord,
            "9001",
            "co1234",
            GameImageType.Cover);

        var duplicateImage = new GameImage(
            externalGameRecord,
            "9001",
            "co5678",
            GameImageType.Cover);

        await using var dbContext = _fixture.CreateDbContext();

        dbContext.Games.Add(game);
        dbContext.DataSources.Add(dataSource);
        dbContext.ExternalGameRecords.Add(externalGameRecord);
        dbContext.GameImages.AddRange(firstImage, duplicateImage);

        // Act
        var action = async () => await dbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveChanges_ShouldAllowSameExternalIdForDifferentImageTypes()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var game = new Game("Metroid Prime");
        var dataSource = CreateDataSource();

        var externalGameRecord = new ExternalGameRecord(
            dataSource.Id,
            "1234",
            DateTimeOffset.UtcNow);

        externalGameRecord.LinkToGame(game.Id);

        var cover = new GameImage(
            externalGameRecord,
            "9001",
            "co1234",
            GameImageType.Cover);

        var screenshot = new GameImage(
            externalGameRecord,
            "9001",
            "sc1234",
            GameImageType.Screenshot);

        await using var dbContext = _fixture.CreateDbContext();

        dbContext.Games.Add(game);
        dbContext.DataSources.Add(dataSource);
        dbContext.ExternalGameRecords.Add(externalGameRecord);
        dbContext.GameImages.AddRange(cover, screenshot);

        // Act
        await dbContext.SaveChangesAsync();

        var persistedCount = await dbContext.GameImages.CountAsync();

        // Assert
        persistedCount.ShouldBe(2);
    }

    [Fact]
    public async Task SaveChanges_ShouldRejectDeletingGame_WhenImageReferencesIt()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var game = new Game("Metroid Prime");
        var dataSource = CreateDataSource();

        var externalGameRecord = new ExternalGameRecord(
            dataSource.Id,
            "1234",
            DateTimeOffset.UtcNow);

        externalGameRecord.LinkToGame(game.Id);

        var image = new GameImage(
            externalGameRecord,
            "9001",
            "co1234",
            GameImageType.Cover);

        await using (var writeDbContext = _fixture.CreateDbContext())
        {
            writeDbContext.Games.Add(game);
            writeDbContext.DataSources.Add(dataSource);
            writeDbContext.ExternalGameRecords.Add(externalGameRecord);
            writeDbContext.GameImages.Add(image);

            await writeDbContext.SaveChangesAsync();
        }

        await using var deleteDbContext = _fixture.CreateDbContext();

        var persistedGame = await deleteDbContext.Games
            .SingleAsync(entity => entity.Id == game.Id);

        deleteDbContext.Games.Remove(persistedGame);

        // Act
        var action = async () => await deleteDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveChanges_ShouldRejectDeletingExternalGameRecord_WhenImageReferencesIt()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var game = new Game("Metroid Prime");
        var dataSource = CreateDataSource();

        var externalGameRecord = new ExternalGameRecord(
            dataSource.Id,
            "1234",
            DateTimeOffset.UtcNow);

        externalGameRecord.LinkToGame(game.Id);

        var image = new GameImage(
            externalGameRecord,
            "9001",
            "co1234",
            GameImageType.Cover);

        await using (var writeDbContext = _fixture.CreateDbContext())
        {
            writeDbContext.Games.Add(game);
            writeDbContext.DataSources.Add(dataSource);
            writeDbContext.ExternalGameRecords.Add(externalGameRecord);
            writeDbContext.GameImages.Add(image);

            await writeDbContext.SaveChangesAsync();
        }

        await using var deleteDbContext = _fixture.CreateDbContext();

        var persistedExternalGameRecord =
            await deleteDbContext.ExternalGameRecords
                .SingleAsync(
                    entity => entity.Id == externalGameRecord.Id);

        deleteDbContext.ExternalGameRecords.Remove(
            persistedExternalGameRecord);

        // Act
        var action = async () => await deleteDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    private static DataSource CreateDataSource()
    {
        var reliability = new SourceReliability(
            ReliabilityLevel.PublicDirect,
            "Dados obtidos diretamente de uma fonte pública.");

        return new DataSource(
            code: $"test-{Guid.NewGuid():N}",
            name: "IGDB",
            url: "https://www.igdb.com",
            reliability,
            attributionRequired: true);
    }
}