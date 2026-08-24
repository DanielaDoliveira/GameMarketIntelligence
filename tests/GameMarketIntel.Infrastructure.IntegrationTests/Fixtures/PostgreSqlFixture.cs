using GameMarketIntel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container =
        new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithDatabase("gamemarketintel_tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

    public GameMarketIntelDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GameMarketIntelDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        return new GameMarketIntelDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var dbContext = CreateDbContext();

        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    public async Task ResetDatabaseAsync()
    {
        await using var dbContext = CreateDbContext();

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            TRUNCATE TABLE
                game_product_relations,
                game_companies,
                game_collections,
                game_themes,
                game_game_modes,
                game_player_perspectives,
                game_keywords,
                game_releases,
                external_company_records,
                external_collection_records,
                external_theme_records,
                external_game_mode_records,
                external_player_perspective_records,
                external_keyword_records,
                external_game_records,
                companies,
                collections,
                themes,
                game_modes,
                player_perspectives,
                keywords,
                data_sources,
                "GameGenres",
                "GamePlatforms",
                "Games",
                "Platforms",
                "Genres"
            RESTART IDENTITY CASCADE;
            """);
    }
}