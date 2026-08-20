using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMarketIntel.Infrastructure.Persistence.Configurations;

public sealed class KeywordConfiguration
    : IEntityTypeConfiguration<Keyword>
{
    public void Configure(EntityTypeBuilder<Keyword> builder)
    {
        builder.ToTable("keywords");

        builder.HasKey(keyword => keyword.Id);

        builder.Property(keyword => keyword.Id)
            .HasColumnName("id");

        builder.Property(keyword => keyword.Name)
            .HasColumnName("name")
            .HasMaxLength(Keyword.MaxNameLength)
            .IsRequired();

        builder.Property(keyword => keyword.NormalizedName)
            .HasColumnName("normalized_name")
            .HasMaxLength(Keyword.MaxNameLength)
            .IsRequired();

        builder.HasIndex(keyword => keyword.NormalizedName)
            .IsUnique()
            .HasDatabaseName("ux_keywords_normalized_name");
    }
}