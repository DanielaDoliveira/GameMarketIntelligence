using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameMarketIntel.Infrastructure.Persistence;

public sealed class GameMarketIntelDbContext(DbContextOptions<GameMarketIntelDbContext> options) : DbContext(options)
{
    public DbSet<DataSource> DataSources => Set<DataSource>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly
            (
                typeof(GameMarketIntelDbContext).Assembly
            );

        base.OnModelCreating(modelBuilder);
    }
}