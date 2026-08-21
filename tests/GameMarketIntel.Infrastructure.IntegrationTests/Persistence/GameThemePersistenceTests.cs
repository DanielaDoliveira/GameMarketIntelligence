using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Domain.ValueObjects;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence;

[Collection(PostgreSqlCollection.Name)]
public sealed class GameThemePersistenceTests(PostgreSqlFixture fixture)
{
    private static readonly DateTimeOffset ObservedAt = new(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task SaveAndLoad_ShouldPersistGameThemeWithCompleteProvenance()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var theme = new Theme("Fantasy");

        var externalGameRecord = CreateExternalGameRecord(source, game, "144542");

        var externalThemeRecord = CreateExternalThemeRecord(source, theme, "17");

        var association = new GameTheme(game.Id, theme.Id, externalGameRecord, externalThemeRecord);

        await using (var writeDbContext = fixture.CreateDbContext())
        {
            writeDbContext.DataSources.Add(source);
            writeDbContext.Games.Add(game);
            writeDbContext.Themes.Add(theme);
            writeDbContext.ExternalGameRecords.Add(externalGameRecord);
            writeDbContext.ExternalThemeRecords.Add(externalThemeRecord);
            writeDbContext.GameThemes.Add(association);

            await writeDbContext.SaveChangesAsync();
        }

        // Act
        await using var readDbContext = fixture.CreateDbContext();

        var persistedAssociation = await readDbContext.GameThemes.SingleAsync();

        // Assert
        persistedAssociation.GameId.ShouldBe(game.Id);
        persistedAssociation.ThemeId.ShouldBe(theme.Id);
        persistedAssociation.ExternalGameRecordId.ShouldBe(externalGameRecord.Id);
        persistedAssociation.ExternalThemeRecordId.ShouldBe(externalThemeRecord.Id);
    }

    [Fact]
    public async Task SaveChanges_ShouldRejectDuplicateExternalContribution()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var theme = new Theme("Fantasy");

        var externalGameRecord = CreateExternalGameRecord(source, game, "144542");

        var externalThemeRecord = CreateExternalThemeRecord(source, theme, "17");

        var firstAssociation = new GameTheme(game.Id, theme.Id, externalGameRecord, externalThemeRecord);

        await using (var firstDbContext = fixture.CreateDbContext())
        {
            firstDbContext.DataSources.Add(source);
            firstDbContext.Games.Add(game);
            firstDbContext.Themes.Add(theme);
            firstDbContext.ExternalGameRecords.Add(externalGameRecord);
            firstDbContext.ExternalThemeRecords.Add(externalThemeRecord);
            firstDbContext.GameThemes.Add(firstAssociation);

            await firstDbContext.SaveChangesAsync();
        }

        await using var duplicateDbContext = fixture.CreateDbContext();

        var persistedExternalGameRecord = await duplicateDbContext.ExternalGameRecords.SingleAsync();

        var persistedExternalThemeRecord = await duplicateDbContext.ExternalThemeRecords.SingleAsync();

        var duplicateAssociation = new GameTheme(game.Id, theme.Id, persistedExternalGameRecord, persistedExternalThemeRecord);

        duplicateDbContext.GameThemes.Add(duplicateAssociation);

        // Act
        var action = () => duplicateDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveChanges_ShouldAllowSameCanonicalThemeFromDifferentExternalContributions()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var theme = new Theme("Fantasy");

        var firstExternalGameRecord = CreateExternalGameRecord(source, game, "144542");

        var secondExternalGameRecord = CreateExternalGameRecord(source, game, "398521");

        var firstExternalThemeRecord = CreateExternalThemeRecord(source, theme, "17");

        var secondExternalThemeRecord = CreateExternalThemeRecord(source, theme, "117");

        var firstAssociation = new GameTheme(game.Id, theme.Id, firstExternalGameRecord, firstExternalThemeRecord);

        var secondAssociation = new GameTheme(game.Id, theme.Id, secondExternalGameRecord, secondExternalThemeRecord);

        // Act
        await using (var dbContext = fixture.CreateDbContext())
        {
            dbContext.DataSources.Add(source);
            dbContext.Games.Add(game);
            dbContext.Themes.Add(theme);

            dbContext.ExternalGameRecords.AddRange(firstExternalGameRecord, secondExternalGameRecord);

            dbContext.ExternalThemeRecords.AddRange(firstExternalThemeRecord, secondExternalThemeRecord);

            dbContext.GameThemes.AddRange(firstAssociation, secondAssociation);

            await dbContext.SaveChangesAsync();
        }

        await using var readDbContext = fixture.CreateDbContext();

        var associationCount = await readDbContext.GameThemes.CountAsync();

        // Assert
        associationCount.ShouldBe(2);
    }

    [Fact]
    public async Task DeleteExternalGameRecord_ShouldFail_WhenAssociationReferencesIt()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        await SeedAssociationAsync();

        await using var deleteDbContext = fixture.CreateDbContext();

        var persistedExternalGameRecord = await deleteDbContext.ExternalGameRecords.SingleAsync();

        deleteDbContext.ExternalGameRecords.Remove(persistedExternalGameRecord);

        // Act
        var action = () => deleteDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task DeleteExternalThemeRecord_ShouldFail_WhenAssociationReferencesIt()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        await SeedAssociationAsync();

        await using var deleteDbContext = fixture.CreateDbContext();

        var persistedExternalThemeRecord = await deleteDbContext.ExternalThemeRecords.SingleAsync();

        deleteDbContext.ExternalThemeRecords.Remove(persistedExternalThemeRecord);

        // Act
        var action = () => deleteDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task DeleteTheme_ShouldFail_WhenAssociationReferencesIt()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        await SeedAssociationAsync();

        await using var deleteDbContext = fixture.CreateDbContext();

        var persistedTheme = await deleteDbContext.Themes.SingleAsync();

        deleteDbContext.Themes.Remove(persistedTheme);

        // Act
        var action = () => deleteDbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }

    private async Task SeedAssociationAsync()
    {
        var source = CreateDataSource();
        var game = new Game("Kitaria Fables");
        var theme = new Theme("Fantasy");

        var externalGameRecord = CreateExternalGameRecord(source, game, "144542");

        var externalThemeRecord = CreateExternalThemeRecord(source, theme, "17");

        var association = new GameTheme(game.Id, theme.Id, externalGameRecord, externalThemeRecord);

        await using var dbContext = fixture.CreateDbContext();

        dbContext.DataSources.Add(source);
        dbContext.Games.Add(game);
        dbContext.Themes.Add(theme);
        dbContext.ExternalGameRecords.Add(externalGameRecord);
        dbContext.ExternalThemeRecords.Add(externalThemeRecord);
        dbContext.GameThemes.Add(association);

        await dbContext.SaveChangesAsync();
    }

    private static ExternalGameRecord CreateExternalGameRecord(DataSource source, Game game, string externalId)
    {
        var externalGameRecord = new ExternalGameRecord(source.Id, externalId, ObservedAt);

        externalGameRecord.LinkToGame(game.Id);

        return externalGameRecord;
    }

    private static ExternalThemeRecord CreateExternalThemeRecord(DataSource source, Theme theme, string externalId)
    {
        var externalThemeRecord = new ExternalThemeRecord(source.Id, externalId, ObservedAt);

        externalThemeRecord.LinkToTheme(theme.Id);

        return externalThemeRecord;
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