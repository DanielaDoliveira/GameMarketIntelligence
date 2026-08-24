using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence;

[Collection(PostgreSqlCollection.Name)]
public sealed class GameProductRelationPersistenceTests
{
    private readonly PostgreSqlFixture _fixture;

    public GameProductRelationPersistenceTests(PostgreSqlFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task SaveAndLoad_ShouldPersistProductRelationWithProvenance()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var dataSource = CreateDataSource();

        var sourceGame = new Game(
            name: "The Legend of Zelda: Ocarina of Time 3D",
            productType: GameProductType.Remake);

        var targetGame = new Game(
            name: "The Legend of Zelda: Ocarina of Time",
            productType: GameProductType.MainGame);

        var observedAt = DateTimeOffset.UtcNow;

        var sourceExternalGameRecord = new ExternalGameRecord(
            dataSource.Id,
            "source-1",
            observedAt);

        var targetExternalGameRecord = new ExternalGameRecord(
            dataSource.Id,
            "target-1",
            observedAt);

        sourceExternalGameRecord.LinkToGame(sourceGame.Id);
        targetExternalGameRecord.LinkToGame(targetGame.Id);

        var relation = new GameProductRelation(
            sourceExternalGameRecord,
            targetExternalGameRecord,
            GameProductRelationType.RemakeOf);

        // Act
        await using (var writeDbContext = _fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(dataSource);
            writeDbContext.Games.AddRange(sourceGame, targetGame);

            writeDbContext.ExternalGameRecords.AddRange(
                sourceExternalGameRecord,
                targetExternalGameRecord);

            writeDbContext.GameProductRelations.Add(relation);

            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = _fixture.CreateDbContext();

        var persistedRelation = await readDbContext.GameProductRelations
            .SingleAsync();

        // Assert
        persistedRelation.Id.ShouldBe(relation.Id);
        persistedRelation.SourceGameId.ShouldBe(sourceGame.Id);
        persistedRelation.TargetGameId.ShouldBe(targetGame.Id);
        persistedRelation.ExternalSourceGameRecordId.ShouldBe(sourceExternalGameRecord.Id);
        persistedRelation.ExternalTargetGameRecordId.ShouldBe(targetExternalGameRecord.Id);
        persistedRelation.RelationType.ShouldBe(GameProductRelationType.RemakeOf);
    }

    private static DataSource CreateDataSource()
    {
        var reliability = new SourceReliability(
            ReliabilityLevel.PublicDirect,
            "Dados obtidos diretamente de uma fonte pública.");

        return new DataSource(
            code: "igdb",
            name: "IGDB",
            url: "https://www.igdb.com",
            reliability,
            attributionRequired: true);
    }
}