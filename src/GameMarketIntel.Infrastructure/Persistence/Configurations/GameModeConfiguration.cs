using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMarketIntel.Infrastructure.Persistence.Configurations;

public sealed class GameModeConfiguration
    : IEntityTypeConfiguration<GameMode>
{
    public void Configure(EntityTypeBuilder<GameMode> builder)
    {
        builder.ToTable("game_modes");

        builder.HasKey(gameMode => gameMode.Id);

        builder.Property(gameMode => gameMode.Id)
            .HasColumnName("id");

        builder.Property(gameMode => gameMode.Name)
            .HasColumnName("name")
            .HasMaxLength(GameMode.MaxNameLength)
            .IsRequired();

        builder.Property(gameMode => gameMode.NormalizedName)
            .HasColumnName("normalized_name")
            .HasMaxLength(GameMode.MaxNameLength)
            .IsRequired();

        builder.HasIndex(gameMode => gameMode.NormalizedName)
            .IsUnique()
            .HasDatabaseName("ux_game_modes_normalized_name");
    }
}