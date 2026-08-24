using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMarketIntel.Infrastructure.Persistence.Configurations;

public sealed class ExternalCollectionRecordConfiguration : IEntityTypeConfiguration<ExternalCollectionRecord>
{
    public void Configure(EntityTypeBuilder<ExternalCollectionRecord> builder)
    {
        builder.ToTable("external_collection_records");

        builder.HasKey(record => record.Id);

        builder.Property(record => record.Id)
            .HasColumnName("id");

        builder.Property(record => record.DataSourceId)
            .HasColumnName("data_source_id")
            .IsRequired();

        builder.Property(record => record.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(ExternalCollectionRecord.MaxExternalIdLength)
            .IsRequired();

        builder.Property(record => record.CollectionId)
            .HasColumnName("collection_id");

        builder.Property(record => record.FirstSeenAt)
            .HasColumnName("first_seen_at")
            .IsRequired();

        builder.Property(record => record.LastSeenAt)
            .HasColumnName("last_seen_at")
            .IsRequired();

        builder.Property(record => record.SourceUpdatedAt)
            .HasColumnName("source_updated_at");

        builder.HasOne<DataSource>()
            .WithMany()
            .HasForeignKey(record => record.DataSourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Collection>()
            .WithMany()
            .HasForeignKey(record => record.CollectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(record => new { record.DataSourceId, record.ExternalId })
            .IsUnique()
            .HasDatabaseName("ux_external_collection_records_source_external_id");

        builder.HasIndex(record => record.CollectionId)
            .HasDatabaseName("ix_external_collection_records_collection_id");
    }
}