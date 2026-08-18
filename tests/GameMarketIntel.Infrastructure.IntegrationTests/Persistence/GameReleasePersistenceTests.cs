using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence;

[Collection(PostgreSqlCollection.Name)]
public sealed class GameReleasePersistenceTests
{
    private const string ExternalReleaseId =
        "release-144542-switch";

    private static readonly DateTimeOffset ObservedAt =
        new(2026, 8, 18, 12, 0, 0, TimeSpan.Zero);

    private readonly PostgreSqlFixture _fixture;

    public GameReleasePersistenceTests(
        PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SaveAndLoad_ShouldPersistContextualRelease()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var platform = CreatePlatform();
        var externalRecord = CreateExternalRecord(source, game);

        var release = GameRelease.Create(
            game.Id,
            platform.Id,
            externalRecord.Id,
            ExternalReleaseId,
            ReleaseDateValue.ForDay(
                new DateOnly(2021, 9, 1)),
            ObservedAt);

        release.SetRegionCode("WW");
        release.SetStatus(GameReleaseStatus.Released);
        release.SetEcosystem("Nintendo eShop");

        // Act
        await using (var writeDbContext =
            _fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(source);
            writeDbContext.Games.Add(game);
            writeDbContext.Platforms.Add(platform);

            writeDbContext.ExternalGameRecords.Add(
                externalRecord);

            writeDbContext.GameReleases.Add(release);

            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext =
            _fixture.CreateDbContext();

        var persistedRelease = await readDbContext
            .GameReleases
            .SingleAsync();

        // Assert
        persistedRelease.Id.ShouldBe(release.Id);
        persistedRelease.GameId.ShouldBe(game.Id);
        persistedRelease.PlatformId.ShouldBe(platform.Id);

        persistedRelease.ExternalGameRecordId.ShouldBe(
            externalRecord.Id);

        persistedRelease.ExternalReleaseId.ShouldBe(
            ExternalReleaseId);

        persistedRelease.ReleaseDate.Kind.ShouldBe(
            ReleaseDateKind.Day);

        persistedRelease.ReleaseDate.Year.ShouldBe(2021);
        persistedRelease.ReleaseDate.Month.ShouldBe(9);
        persistedRelease.ReleaseDate.Day.ShouldBe(1);
        persistedRelease.ReleaseDate.Quarter.ShouldBeNull();
        persistedRelease.RegionCode.ShouldBe("WW");

        persistedRelease.Status.ShouldBe(
            GameReleaseStatus.Released);

        persistedRelease.Ecosystem.ShouldBe(
            "Nintendo eShop");

        persistedRelease.ObservedAt.ShouldBe(ObservedAt);
    }

    [Fact]
    public async Task SaveChanges_ShouldRejectDuplicateExternalReleaseIdWithinSameExternalRecord()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var platform = CreatePlatform();
        var externalRecord = CreateExternalRecord(source, game);

        var firstRelease = GameRelease.Create(
            game.Id,
            platform.Id,
            externalRecord.Id,
            ExternalReleaseId,
            ReleaseDateValue.ForDay(
                new DateOnly(2021, 9, 1)),
            ObservedAt);

        var duplicateRelease = GameRelease.Create(
            game.Id,
            platform.Id,
            externalRecord.Id,
            $" {ExternalReleaseId} ",
            ReleaseDateValue.ForDay(
                new DateOnly(2021, 9, 1)),
            ObservedAt);

        await using var dbContext =
            _fixture.CreateDbContext();

        dbContext.DataSources.Add(source);
        dbContext.Games.Add(game);
        dbContext.Platforms.Add(platform);

        dbContext.ExternalGameRecords.Add(
            externalRecord);

        dbContext.GameReleases.AddRange(
            firstRelease,
            duplicateRelease);

        // Act
        var action = async () =>
            await dbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveChanges_ShouldRestrictDeletingReferencedPlatform()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var platform = CreatePlatform();
        var externalRecord = CreateExternalRecord(source, game);

        var release = GameRelease.Create(
            game.Id,
            platform.Id,
            externalRecord.Id,
            ExternalReleaseId,
            ReleaseDateValue.ForYear(2021),
            ObservedAt);

        await using (var writeDbContext =
            _fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(source);
            writeDbContext.Games.Add(game);
            writeDbContext.Platforms.Add(platform);

            writeDbContext.ExternalGameRecords.Add(
                externalRecord);

            writeDbContext.GameReleases.Add(release);

            await writeDbContext.SaveChangesAsync();
        }

        await using var deleteDbContext =
            _fixture.CreateDbContext();

        var persistedPlatform = await deleteDbContext
            .Platforms
            .SingleAsync();

        deleteDbContext.Platforms.Remove(
            persistedPlatform);

        // Act
        var action = async () =>
            await deleteDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveChanges_ShouldRestrictDeletingReferencedExternalGameRecord()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var platform = CreatePlatform();
        var externalRecord = CreateExternalRecord(source, game);

        var release = GameRelease.Create(
            game.Id,
            platform.Id,
            externalRecord.Id,
            ExternalReleaseId,
            ReleaseDateValue.ForYear(2021),
            ObservedAt);

        await using (var writeDbContext =
            _fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(source);
            writeDbContext.Games.Add(game);
            writeDbContext.Platforms.Add(platform);

            writeDbContext.ExternalGameRecords.Add(
                externalRecord);

            writeDbContext.GameReleases.Add(release);

            await writeDbContext.SaveChangesAsync();
        }

        await using var deleteDbContext =
            _fixture.CreateDbContext();

        var persistedExternalRecord =
            await deleteDbContext
                .ExternalGameRecords
                .SingleAsync();

        deleteDbContext.ExternalGameRecords.Remove(
            persistedExternalRecord);

        // Act
        var action = async () =>
            await deleteDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveAndLoad_ShouldPersistToBeDeterminedReleaseWithNullOptionalFields()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var source = CreateDataSource();
        var game = new Game("Future Game");
        var platform = CreatePlatform();
        var externalRecord = CreateExternalRecord(source, game);

        var release = GameRelease.Create(
            game.Id,
            platform.Id,
            externalRecord.Id,
            "future-release-tbd",
            ReleaseDateValue.ToBeDetermined(),
            ObservedAt);

        // Act
        await using (var writeDbContext =
            _fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(source);
            writeDbContext.Games.Add(game);
            writeDbContext.Platforms.Add(platform);

            writeDbContext.ExternalGameRecords.Add(
                externalRecord);

            writeDbContext.GameReleases.Add(release);

            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext =
            _fixture.CreateDbContext();

        var persistedRelease = await readDbContext
            .GameReleases
            .SingleAsync();

        // Assert
        persistedRelease.ReleaseDate.Kind.ShouldBe(
            ReleaseDateKind.ToBeDetermined);

        persistedRelease.ReleaseDate.Year.ShouldBeNull();
        persistedRelease.ReleaseDate.Month.ShouldBeNull();
        persistedRelease.ReleaseDate.Day.ShouldBeNull();
        persistedRelease.ReleaseDate.Quarter.ShouldBeNull();
        persistedRelease.RegionCode.ShouldBeNull();
        persistedRelease.Status.ShouldBeNull();
        persistedRelease.Ecosystem.ShouldBeNull();
    }

    private static ExternalGameRecord CreateExternalRecord(
        DataSource source,
        Game game)
    {
        var externalRecord = new ExternalGameRecord(
            source.Id,
            externalId: "144542",
            ObservedAt);

        externalRecord.LinkToGame(game.Id);

        return externalRecord;
    }

    private static Platform CreatePlatform()
    {
        return new Platform(
            name: "Nintendo Switch",
            family: "Nintendo Switch",
            manufacturer: "Nintendo");
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
