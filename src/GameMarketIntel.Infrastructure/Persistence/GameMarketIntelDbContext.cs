using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameMarketIntel.Infrastructure.Persistence;

public sealed class GameMarketIntelDbContext(DbContextOptions<GameMarketIntelDbContext> options) : DbContext(options)
{
    public DbSet<DataSource> DataSources => Set<DataSource>();

    public DbSet<ExternalGameRecord> ExternalGameRecords => Set<ExternalGameRecord>();

    public DbSet<ExternalThemeRecord> ExternalThemeRecords => Set<ExternalThemeRecord>();

    public DbSet<ExternalGameModeRecord> ExternalGameModeRecords => Set<ExternalGameModeRecord>();

    public DbSet<ExternalPlayerPerspectiveRecord> ExternalPlayerPerspectiveRecords
        => Set<ExternalPlayerPerspectiveRecord>();

    public DbSet<ExternalKeywordRecord> ExternalKeywordRecords => Set<ExternalKeywordRecord>();

    public DbSet<ExternalCompanyRecord> ExternalCompanyRecords => Set<ExternalCompanyRecord>();

    public DbSet<GameRelease> GameReleases => Set<GameRelease>();

    public DbSet<GameProductRelation> GameProductRelations => Set<GameProductRelation>();

    public DbSet<Game> Games => Set<Game>();

    public DbSet<Genre> Genres => Set<Genre>();

    public DbSet<Platform> Platforms => Set<Platform>();

    public DbSet<Company> Companies => Set<Company>();

    public DbSet<Theme> Themes => Set<Theme>();

    public DbSet<GameMode> GameModes => Set<GameMode>();

    public DbSet<PlayerPerspective> PlayerPerspectives => Set<PlayerPerspective>();

    public DbSet<Keyword> Keywords => Set<Keyword>();

    public DbSet<GameTheme> GameThemes => Set<GameTheme>();

    public DbSet<GameGameMode> GameGameModes => Set<GameGameMode>();

    public DbSet<GamePlayerPerspective> GamePlayerPerspectives
        => Set<GamePlayerPerspective>();

    public DbSet<GameKeyword> GameKeywords => Set<GameKeyword>();

    public DbSet<GameCompany> GameCompanies => Set<GameCompany>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(GameMarketIntelDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}