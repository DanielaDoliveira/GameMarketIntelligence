using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMarketIntel.Infrastructure.Persistence.Configurations;

public sealed class CollectionConfiguration : IEntityTypeConfiguration<Collection>
{
    public void Configure(EntityTypeBuilder<Collection> builder)
    {
        builder.ToTable("collections");

        builder.HasKey(collection => collection.Id);

        builder.Property(collection => collection.Id)
            .HasColumnName("id");

        builder.Property(collection => collection.Name)
            .HasColumnName("name")
            .HasMaxLength(Collection.MaxNameLength)
            .IsRequired();

        builder.Property(collection => collection.NormalizedName)
            .HasColumnName("normalized_name")
            .HasMaxLength(Collection.MaxNameLength)
            .IsRequired();

        builder.HasIndex(collection => collection.NormalizedName)
            .IsUnique()
            .HasDatabaseName("ux_collections_normalized_name");
    }
}