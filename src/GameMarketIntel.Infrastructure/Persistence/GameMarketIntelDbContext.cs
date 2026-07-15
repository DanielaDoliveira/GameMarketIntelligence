using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameMarketIntel.Infrastructure.Persistence;

public sealed class GameMarketIntelDbContext(DbContextOptions<GameMarketIntelDbContext> options) : DbContext(options)
{
    public DbSet<DataSource> DataSources => Set<DataSource>();

    public DbSet<Game> Games => Set<Game>();

    public DbSet<Genre> Genres => Set<Genre>();

    public DbSet<Platform> Platforms => Set<Platform>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly
            (
                typeof(GameMarketIntelDbContext).Assembly
            );

        base.OnModelCreating(modelBuilder);
    }
}