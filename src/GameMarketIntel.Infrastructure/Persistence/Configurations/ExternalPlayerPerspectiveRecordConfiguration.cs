using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMarketIntel.Infrastructure.Persistence.Configurations;

public sealed class ExternalPlayerPerspectiveRecordConfiguration : IEntityTypeConfiguration<ExternalPlayerPerspectiveRecord>
{
    public void Configure(EntityTypeBuilder<ExternalPlayerPerspectiveRecord> builder)
    {
        builder.ToTable("external_player_perspective_records");

        builder.HasKey(record => record.Id);

        builder.Property(record => record.Id)
            .HasColumnName("id");

        builder.Property(record => record.DataSourceId)
            .HasColumnName("data_source_id")
            .IsRequired();

        builder.Property(record => record.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(ExternalPlayerPerspectiveRecord.MaxExternalIdLength)
            .IsRequired();

        builder.Property(record => record.PlayerPerspectiveId)
            .HasColumnName("player_perspective_id");

        builder.Property(record => record.FirstSeenAt)
            .HasColumnName("first_seen_at")
            .IsRequired();

        builder.Property(record => record.LastSeenAt)
            .HasColumnName("last_seen_at")
            .IsRequired();

        builder.Property(record => record.SourceUpdatedAt)
            .HasColumnName("source_updated_at");

        builder.HasIndex(record => new
            {
                record.DataSourceId,
                record.ExternalId
            })
            .IsUnique()
            .HasDatabaseName("ux_external_player_perspective_records_source_external_id");

        builder.HasOne<DataSource>()
            .WithMany()
            .HasForeignKey(record => record.DataSourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PlayerPerspective>()
            .WithMany()
            .HasForeignKey(record => record.PlayerPerspectiveId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}