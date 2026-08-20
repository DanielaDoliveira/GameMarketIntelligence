using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence;

[Collection(PostgreSqlCollection.Name)]
public sealed class PlayerPerspectivePersistenceTests
{
    private readonly PostgreSqlFixture _fixture;

    public PlayerPerspectivePersistenceTests(PostgreSqlFixture fixture)=>_fixture = fixture;
    

    [Fact]
    public async Task SaveAndLoad_ShouldPersistPlayerPerspective()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var playerPerspective = new PlayerPerspective("First person");

        // Act
        await using (var writeDbContext = _fixture.CreateDbContext())
        {
            writeDbContext.PlayerPerspectives.Add(playerPerspective);
            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = _fixture.CreateDbContext();
        var persistedPlayerPerspective = await readDbContext.PlayerPerspectives.SingleAsync();

        // Assert
        persistedPlayerPerspective.Id.ShouldBe(playerPerspective.Id);
        persistedPlayerPerspective.Name.ShouldBe("First person");
        persistedPlayerPerspective.NormalizedName.ShouldBe("FIRST PERSON");
    }

    [Fact]
    public async Task SaveChanges_ShouldRejectDuplicateNormalizedName()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var firstPlayerPerspective = new PlayerPerspective("Third person");
        var duplicatePlayerPerspective = new PlayerPerspective("tHiRd PeRsOn");

        await using var dbContext = _fixture.CreateDbContext();
        dbContext.PlayerPerspectives.AddRange(firstPlayerPerspective, duplicatePlayerPerspective);

        // Act
        var action = () => dbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }
}