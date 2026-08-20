using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence;

[Collection(PostgreSqlCollection.Name)]
public sealed class GameModePersistenceTests
{
    private readonly PostgreSqlFixture _fixture;

    public GameModePersistenceTests(PostgreSqlFixture fixture) => _fixture = fixture;
    

    [Fact]
    public async Task SaveAndLoad_ShouldPersistGameMode()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var gameMode = new GameMode("Single player");

        // Act
        await using (var writeDbContext = _fixture.CreateDbContext())
        {
            writeDbContext.GameModes.Add(gameMode);
            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = _fixture.CreateDbContext();
        var persistedGameMode = await readDbContext.GameModes.SingleAsync();

        // Assert
        persistedGameMode.Id.ShouldBe(gameMode.Id);
        persistedGameMode.Name.ShouldBe("Single player");
        persistedGameMode.NormalizedName.ShouldBe("SINGLE PLAYER");
    }

    [Fact]
    public async Task SaveChanges_ShouldRejectDuplicateNormalizedName()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var firstGameMode = new GameMode("Multiplayer");
        var duplicateGameMode = new GameMode("mUlTiPlAyEr");

        await using var dbContext = _fixture.CreateDbContext();
        dbContext.GameModes.AddRange(firstGameMode, duplicateGameMode);

        // Act
        var action = () => dbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }
}