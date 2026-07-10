using Microsoft.EntityFrameworkCore;

namespace GameMarketIntel.Infrastructure.Persistence;

public sealed class GameMarketIntelDbContext(
    DbContextOptions<GameMarketIntelDbContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(GameMarketIntelDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}