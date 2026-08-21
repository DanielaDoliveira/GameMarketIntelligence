using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMarketIntel.Infrastructure.Persistence.Configurations;

public sealed class GameGameModeConfiguration : IEntityTypeConfiguration<GameGameMode>
{
    public void Configure(EntityTypeBuilder<GameGameMode> builder)
    {
        builder.ToTable("game_game_modes");

        builder.HasKey(gameGameMode => new
        {
            gameGameMode.ExternalGameRecordId,
            gameGameMode.ExternalGameModeRecordId
        });

        builder.Property(gameGameMode => gameGameMode.GameId)
            .HasColumnName("game_id");

        builder.Property(gameGameMode => gameGameMode.GameModeId)
            .HasColumnName("game_mode_id");

        builder.Property(gameGameMode => gameGameMode.ExternalGameRecordId)
            .HasColumnName("external_game_record_id");

        builder.Property(gameGameMode => gameGameMode.ExternalGameModeRecordId)
            .HasColumnName("external_game_mode_record_id");

        builder.HasOne<Game>()
            .WithMany()
            .HasForeignKey(gameGameMode => gameGameMode.GameId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<GameMode>()
            .WithMany()
            .HasForeignKey(gameGameMode => gameGameMode.GameModeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ExternalGameRecord>()
            .WithMany()
            .HasForeignKey(gameGameMode => gameGameMode.ExternalGameRecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ExternalGameModeRecord>()
            .WithMany()
            .HasForeignKey(gameGameMode => gameGameMode.ExternalGameModeRecordId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}