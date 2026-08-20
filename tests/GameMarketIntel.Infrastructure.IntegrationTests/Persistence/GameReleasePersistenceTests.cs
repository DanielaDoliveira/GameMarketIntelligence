using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence;

[Collection(PostgreSqlCollection.Name)]
public sealed class GameReleasePersistenceTests(PostgreSqlFixture fixture)
{
    private static readonly DateTimeOffset ObservedAt = new(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task SaveAndLoad_ShouldPersistContextualRelease()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var platform = new Platform("Nintendo Switch");
        var externalRecord = CreateExternalRecord(source, game, "144542");

        var release = GameRelease.Create(
            game.Id,
            platform.Id,
            externalRecord,
            "release-144542-switch",
            ReleaseDateValue.ForDay(new DateOnly(2021, 9, 1)), ObservedAt);

        release.SetRegionCode("WW");
        release.SetStatus(GameReleaseStatus.Released);
        release.SetEcosystem("Nintendo eShop");

        // Act
        await using (var writeDbContext = fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(source);
            writeDbContext.Games.Add(game);
            writeDbContext.Platforms.Add(platform);
            writeDbContext.ExternalGameRecords.Add(externalRecord);
            writeDbContext.GameReleases.Add(release);

            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = fixture.CreateDbContext();
        var persistedRelease = await readDbContext.GameReleases.SingleAsync();

        // Assert
        persistedRelease.Id.ShouldBe(release.Id);
        persistedRelease.GameId.ShouldBe(game.Id);
        persistedRelease.PlatformId.ShouldBe(platform.Id);
        persistedRelease.ExternalGameRecordId.ShouldBe(externalRecord.Id);
        persistedRelease.ExternalReleaseId.ShouldBe("release-144542-switch");
        persistedRelease.ReleaseDate.Year.ShouldBe(2021);
        persistedRelease.ReleaseDate.Month.ShouldBe(9);
        persistedRelease.ReleaseDate.Day.ShouldBe(1);
        persistedRelease.RegionCode.ShouldBe("WW");
        persistedRelease.Status.ShouldBe(GameReleaseStatus.Released);
        persistedRelease.Ecosystem.ShouldBe("Nintendo eShop");
        persistedRelease.ObservedAt.ShouldBe(ObservedAt);
    }

    [Fact]
    public async Task SaveChanges_ShouldRejectDuplicateExternalReleaseIdentity()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var platform = new Platform("Nintendo Switch");
        var externalRecord = CreateExternalRecord(source, game, "144542");

        var firstRelease = GameRelease.Create
        (
            game.Id,
            platform.Id,
            externalRecord,
            "release-144542-switch",
            ReleaseDateValue.ForYear(2021),
            ObservedAt
        );

        var duplicateRelease = GameRelease.Create
        (
            game.Id,
            platform.Id,
            externalRecord,
            "release-144542-switch",
            ReleaseDateValue.ForYear(2021),
            ObservedAt
        );

        await using var dbContext = fixture.CreateDbContext();

        dbContext.DataSources.Add(source);
        dbContext.Games.Add(game);
        dbContext.Platforms.Add(platform);
        dbContext.ExternalGameRecords.Add(externalRecord);
        dbContext.GameReleases.AddRange(firstRelease, duplicateRelease);

        // Act
        var action = () => dbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task DeleteExternalGameRecord_ShouldFail_WhenReleaseReferencesIt()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var platform = new Platform("Nintendo Switch");
        var externalRecord = CreateExternalRecord
        (
            source,
            game,
            "144542"
        );

        var release = GameRelease.Create
        (
            game.Id,
            platform.Id,
            externalRecord,
            "release-144542-switch",
            ReleaseDateValue.ForYear(2021),
            ObservedAt
        );

        await using (var setupDbContext = fixture.CreateDbContext())
        {
            setupDbContext.DataSources.Add(source);
            setupDbContext.Games.Add(game);
            setupDbContext.Platforms.Add(platform);
            setupDbContext.ExternalGameRecords.Add(externalRecord);
            setupDbContext.GameReleases.Add(release);

            await setupDbContext.SaveChangesAsync();
        }

        await using var deleteDbContext = fixture.CreateDbContext();

        var persistedExternalRecord = await deleteDbContext
            .ExternalGameRecords
            .SingleAsync();

        deleteDbContext.ExternalGameRecords.Remove(persistedExternalRecord);

        // Act
        var action = () => deleteDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    private static ExternalGameRecord CreateExternalRecord(DataSource source, Game game, string externalId)
    {
        var externalRecord = new ExternalGameRecord(source.Id, externalId, ObservedAt);

        externalRecord.LinkToGame(game.Id);

        return externalRecord;
    }

    private static DataSource CreateDataSource()
    {
        var reliability =
            new SourceReliability
            (
                ReliabilityLevel.PublicDirect,
                "Dados obtidos diretamente de uma fonte pública."
            );

        return new DataSource
        (
            code: "igdb",
            name: "IGDB",
            url: "https://www.igdb.com",
            reliability,
            attributionRequired: true
        );
    }
}