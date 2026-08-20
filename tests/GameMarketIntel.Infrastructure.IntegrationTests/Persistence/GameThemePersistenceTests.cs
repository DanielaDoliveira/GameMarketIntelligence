using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence;

[Collection(PostgreSqlCollection.Name)]
public sealed class GameThemePersistenceTests
{
    private static readonly DateTimeOffset ObservedAt = new(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

    private readonly PostgreSqlFixture _fixture;

    public GameThemePersistenceTests(PostgreSqlFixture fixture)=>_fixture = fixture;
    

    [Fact]
    public async Task SaveAndLoad_ShouldPersistGameThemeWithProvenance()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var theme = new Theme("Fantasy");
        var externalRecord = CreateExternalRecord(source, game, "144542");

        var gameTheme = new GameTheme(game.Id, theme.Id, externalRecord);

        // Act
        await using (var writeDbContext = _fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(source);
            writeDbContext.Games.Add(game);
            writeDbContext.Themes.Add(theme);
            writeDbContext.ExternalGameRecords.Add(externalRecord);
            writeDbContext.GameThemes.Add(gameTheme);

            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = _fixture.CreateDbContext();
        var persistedGameTheme = await readDbContext.GameThemes .SingleAsync();

        // Assert
        persistedGameTheme.GameId.ShouldBe(game.Id);
        persistedGameTheme.ThemeId.ShouldBe(theme.Id);
        persistedGameTheme.ExternalGameRecordId.ShouldBe(externalRecord.Id);
    }

    [Fact]
    public async Task SaveChanges_ShouldRejectDuplicateGameThemeContribution()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var theme = new Theme("Fantasy");
        var externalRecord = CreateExternalRecord(source, game, "144542");

        var firstAssociation = new GameTheme(game.Id, theme.Id, externalRecord);

        var duplicateAssociation = new GameTheme(game.Id, theme.Id, externalRecord);

        await using var dbContext = _fixture.CreateDbContext();

        dbContext.DataSources.Add(source);
        dbContext.Games.Add(game);
        dbContext.Themes.Add(theme);
        dbContext.ExternalGameRecords.Add(externalRecord);
        dbContext.GameThemes.AddRange(firstAssociation, duplicateAssociation);

        // Act
        var action = async () => await dbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SaveChanges_ShouldAllowSameGameAndThemeFromDifferentExternalRecords()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var theme = new Theme("Fantasy");

        var firstExternalRecord = CreateExternalRecord(source, game, "144542");

        var secondExternalRecord = CreateExternalRecord(source, game, "398521");

        var firstAssociation = new GameTheme(game.Id, theme.Id, firstExternalRecord);

        var secondAssociation = new GameTheme(game.Id, theme.Id, secondExternalRecord);

        // Act
        await using (var dbContext = _fixture.CreateDbContext())
        {
            dbContext.DataSources.Add(source);
            dbContext.Games.Add(game);
            dbContext.Themes.Add(theme);
            dbContext.ExternalGameRecords.AddRange(firstExternalRecord, secondExternalRecord);
            dbContext.GameThemes.AddRange(firstAssociation, secondAssociation);

            await dbContext.SaveChangesAsync();
        }

        await using var readDbContext = _fixture.CreateDbContext();

        // Assert
        (await readDbContext.GameThemes.CountAsync()).ShouldBe(2);
    }

    [Fact]
    public async Task SaveChanges_ShouldRestrictDeletingReferencedExternalGameRecord()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        await SeedAssociationAsync();

        await using var deleteDbContext = _fixture.CreateDbContext();

        var persistedExternalRecord = await deleteDbContext.ExternalGameRecords.SingleAsync();

        deleteDbContext.ExternalGameRecords.Remove(persistedExternalRecord);

        // Act
        var action = async () => await deleteDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveChanges_ShouldRestrictDeletingReferencedTheme()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        await SeedAssociationAsync();

        await using var deleteDbContext = _fixture.CreateDbContext();

        var persistedTheme = await deleteDbContext.Themes.SingleAsync();

        deleteDbContext.Themes.Remove(persistedTheme);

        // Act
        var action = async () => await deleteDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    private async Task SeedAssociationAsync()
    {
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var theme = new Theme("Fantasy");
        var externalRecord = CreateExternalRecord(source, game, "144542");

        var gameTheme = new GameTheme(game.Id, theme.Id, externalRecord);

        await using var dbContext = _fixture.CreateDbContext();

        dbContext.DataSources.Add(source);
        dbContext.Games.Add(game);
        dbContext.Themes.Add(theme);
        dbContext.ExternalGameRecords.Add(externalRecord);
        dbContext.GameThemes.Add(gameTheme);

        await dbContext.SaveChangesAsync();
    }

    private static ExternalGameRecord CreateExternalRecord(DataSource source, Game game, string externalId)
    {
        var externalRecord = new ExternalGameRecord(source.Id, externalId, ObservedAt);

        externalRecord.LinkToGame(game.Id);

        return externalRecord;
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