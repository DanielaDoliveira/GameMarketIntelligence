using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMarketIntel.Infrastructure.Persistence.Configurations;

public sealed class PlayerPerspectiveConfiguration : IEntityTypeConfiguration<PlayerPerspective>
{
    public void Configure(EntityTypeBuilder<PlayerPerspective> builder)
    {
        builder.ToTable("player_perspectives");

        builder.HasKey(playerPerspective => playerPerspective.Id);

        builder.Property(playerPerspective => playerPerspective.Id)
            .HasColumnName("id");

        builder.Property(playerPerspective => playerPerspective.Name)
            .HasColumnName("name")
            .HasMaxLength(PlayerPerspective.MaxNameLength)
            .IsRequired();

        builder.Property(playerPerspective => playerPerspective.NormalizedName)
            .HasColumnName("normalized_name")
            .HasMaxLength(PlayerPerspective.MaxNameLength)
            .IsRequired();

        builder.HasIndex(playerPerspective => playerPerspective.NormalizedName)
            .IsUnique()
            .HasDatabaseName("ux_player_perspectives_normalized_name");
    }
}