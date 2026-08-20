using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMarketIntel.Infrastructure.Persistence.Configurations;

public sealed class ExternalGameModeRecordConfiguration : IEntityTypeConfiguration<ExternalGameModeRecord>
{
    public void Configure(EntityTypeBuilder<ExternalGameModeRecord> builder)
    {
        builder.ToTable("external_game_mode_records");

        builder.HasKey(record => record.Id);

        builder.Property(record => record.Id)
            .HasColumnName("id");

        builder.Property(record => record.DataSourceId)
            .HasColumnName("data_source_id")
            .IsRequired();

        builder.Property(record => record.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(ExternalGameModeRecord.MaxExternalIdLength)
            .IsRequired();

        builder.Property(record => record.GameModeId)
            .HasColumnName("game_mode_id");

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
            .HasDatabaseName("ux_external_game_mode_records_source_external_id");

        builder.HasOne<DataSource>()
            .WithMany()
            .HasForeignKey(record => record.DataSourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<GameMode>()
            .WithMany()
            .HasForeignKey(record => record.GameModeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}