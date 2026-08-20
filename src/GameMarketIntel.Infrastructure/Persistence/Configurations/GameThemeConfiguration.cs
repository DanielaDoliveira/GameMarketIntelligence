using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMarketIntel.Infrastructure.Persistence.Configurations;

public sealed class GameThemeConfiguration : IEntityTypeConfiguration<GameTheme>
{
    public void Configure(EntityTypeBuilder<GameTheme> builder)
    {
        builder.ToTable("game_themes");

        builder.HasKey(gameTheme => new
        {
            gameTheme.GameId,
            gameTheme.ThemeId,
            gameTheme.ExternalGameRecordId
        });

        builder.Property(gameTheme => gameTheme.GameId)
            .HasColumnName("game_id");

        builder.Property(gameTheme => gameTheme.ThemeId)
            .HasColumnName("theme_id");

        builder.Property(gameTheme => gameTheme.ExternalGameRecordId)
            .HasColumnName("external_game_record_id");

        builder.HasOne<Game>()
            .WithMany()
            .HasForeignKey(gameTheme => gameTheme.GameId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Theme>()
            .WithMany()
            .HasForeignKey(gameTheme => gameTheme.ThemeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ExternalGameRecord>()
            .WithMany()
            .HasForeignKey(gameTheme => gameTheme.ExternalGameRecordId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}