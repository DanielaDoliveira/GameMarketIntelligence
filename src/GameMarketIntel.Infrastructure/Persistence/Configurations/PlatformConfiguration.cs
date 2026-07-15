using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMarketIntel.Infrastructure.Persistence.Configurations;

public sealed class PlatformConfiguration
    : IEntityTypeConfiguration<Platform>
{
    public void Configure(EntityTypeBuilder<Platform> builder)
    {
        builder.ToTable("Platforms");

        builder.HasKey(platform => platform.Id);

        builder.Property(platform => platform.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(platform => platform.NormalizedName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(platform => platform.Family)
            .HasMaxLength(100);

        builder.Property(platform => platform.Manufacturer)
            .HasMaxLength(100);

        builder.Property(platform => platform.ImageUrl)
            .HasMaxLength(2048);

        builder.HasIndex(platform => platform.NormalizedName)
            .IsUnique();
    }
}