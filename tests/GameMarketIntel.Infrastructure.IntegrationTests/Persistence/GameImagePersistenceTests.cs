using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence;

[Collection(PostgreSqlCollection.Name)]
public sealed class GameImagePersistenceTests
{
    private readonly PostgreSqlFixture _fixture;

    public GameImagePersistenceTests(PostgreSqlFixture fixture) =>
        _fixture = fixture;

    [Fact]
    public async Task SaveAndLoad_ShouldPersistGameImageMetadataWithProvenance()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var game = new Game(
            "The Legend of Zelda: Ocarina of Time 3D");

        var dataSource = CreateDataSource();

        var externalGameRecord = new ExternalGameRecord(
            dataSource.Id,
            "1022",
            DateTimeOffset.UtcNow);

        externalGameRecord.LinkToGame(game.Id);

        var image = new GameImage(
            externalGameRecord,
            "84231",
            "co1abc",
            GameImageType.Cover,
            width: 264,
            height: 374,
            sortOrder: 0);

        // Act
        await using (var writeDbContext = _fixture.CreateDbContext())
        {
            writeDbContext.Games.Add(game);
            writeDbContext.DataSources.Add(dataSource);
            writeDbContext.ExternalGameRecords.Add(externalGameRecord);
            writeDbContext.GameImages.Add(image);

            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = _fixture.CreateDbContext();

        var persistedImage = await readDbContext.GameImages
            .AsNoTracking()
            .SingleAsync();

        // Assert
        persistedImage.Id.ShouldBe(image.Id);
        persistedImage.GameId.ShouldBe(game.Id);
        persistedImage.ExternalGameRecordId.ShouldBe(
            externalGameRecord.Id);
        persistedImage.ExternalId.ShouldBe("84231");
        persistedImage.SourceImageId.ShouldBe("co1abc");
        persistedImage.Type.ShouldBe(GameImageType.Cover);
        persistedImage.Width.ShouldBe(264);
        persistedImage.Height.ShouldBe(374);
        persistedImage.SortOrder.ShouldBe(0);
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