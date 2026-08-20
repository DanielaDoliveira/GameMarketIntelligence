using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMarketIntel.Infrastructure.Persistence.Configurations;

public sealed class ThemeConfiguration : IEntityTypeConfiguration<Theme>
{
    public void Configure(EntityTypeBuilder<Theme> builder)
    {
        builder.ToTable("themes");

        builder.HasKey(theme => theme.Id);

        builder.Property(theme => theme.Id)
            .HasColumnName("id");

        builder.Property(theme => theme.Name)
            .HasColumnName("name")
            .HasMaxLength(Theme.MaxNameLength)
            .IsRequired();

        builder.Property(theme => theme.NormalizedName)
            .HasColumnName("normalized_name")
            .HasMaxLength(Theme.MaxNameLength)
            .IsRequired();

        builder.HasIndex(theme => theme.NormalizedName)
            .IsUnique()
            .HasDatabaseName("ux_themes_normalized_name");
    }
}