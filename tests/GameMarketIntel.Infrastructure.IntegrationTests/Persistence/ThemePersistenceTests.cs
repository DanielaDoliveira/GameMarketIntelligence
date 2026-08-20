using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence;

[Collection(PostgreSqlCollection.Name)]
public sealed class ThemePersistenceTests
{
    private readonly PostgreSqlFixture _fixture;

    public ThemePersistenceTests(PostgreSqlFixture fixture) => _fixture = fixture;
    

    [Fact]
    public async Task SaveAndLoad_ShouldPersistTheme()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var theme = new Theme("Horror");

        // Act
        await using (var writeDbContext = _fixture.CreateDbContext())
        {
            writeDbContext.Themes.Add(theme);
            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = _fixture.CreateDbContext();
        var persistedTheme = await readDbContext.Themes.SingleAsync();

        // Assert
        persistedTheme.Id.ShouldBe(theme.Id);
        persistedTheme.Name.ShouldBe("Horror");
        persistedTheme.NormalizedName.ShouldBe("HORROR");
    }

    [Fact]
    public async Task SaveChanges_ShouldRejectDuplicateNormalizedName()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var firstTheme = new Theme("Fantasy");
        var duplicateTheme = new Theme("fAnTaSy");

        await using var dbContext = _fixture.CreateDbContext();
        dbContext.Themes.AddRange(firstTheme, duplicateTheme);

        // Act
        var action = () => dbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }
}