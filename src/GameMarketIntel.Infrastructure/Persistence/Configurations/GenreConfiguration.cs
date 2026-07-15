using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMarketIntel.Infrastructure.Persistence.Configurations;

public sealed class GenreConfiguration
    : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        builder.ToTable("Genres");

        builder.HasKey(genre => genre.Id);

        builder.Property(genre => genre.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(genre => genre.NormalizedName)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(genre => genre.NormalizedName)
            .IsUnique();
    }
}