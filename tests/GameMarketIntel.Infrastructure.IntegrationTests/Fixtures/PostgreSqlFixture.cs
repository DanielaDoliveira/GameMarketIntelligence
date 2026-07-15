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

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}