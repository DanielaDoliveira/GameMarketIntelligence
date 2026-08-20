using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMarketIntel.Infrastructure.Persistence.Configurations;

public sealed class GamePlayerPerspectiveConfiguration : IEntityTypeConfiguration<GamePlayerPerspective>
{
    public void Configure(EntityTypeBuilder<GamePlayerPerspective> builder)
    {
        builder.ToTable("game_player_perspectives");

        builder.HasKey(gamePlayerPerspective => new
        {
            gamePlayerPerspective.GameId,
            gamePlayerPerspective.PlayerPerspectiveId,
            gamePlayerPerspective.ExternalGameRecordId
        });

        builder.Property(gamePlayerPerspective => gamePlayerPerspective.GameId)
            .HasColumnName("game_id");

        builder.Property(gamePlayerPerspective => gamePlayerPerspective.PlayerPerspectiveId)
            .HasColumnName("player_perspective_id");

        builder.Property(gamePlayerPerspective => gamePlayerPerspective.ExternalGameRecordId)
            .HasColumnName("external_game_record_id");

        builder.HasOne<Game>()
            .WithMany()
            .HasForeignKey(gamePlayerPerspective => gamePlayerPerspective.GameId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PlayerPerspective>()
            .WithMany()
            .HasForeignKey(gamePlayerPerspective => gamePlayerPerspective.PlayerPerspectiveId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ExternalGameRecord>()
            .WithMany()
            .HasForeignKey(gamePlayerPerspective => gamePlayerPerspective.ExternalGameRecordId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}