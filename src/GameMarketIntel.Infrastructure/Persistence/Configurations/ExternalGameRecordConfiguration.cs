using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMarketIntel.Infrastructure.Persistence.Configurations;

public sealed class ExternalGameRecordConfiguration
    : IEntityTypeConfiguration<ExternalGameRecord>
{
    public void Configure(
        EntityTypeBuilder<ExternalGameRecord> builder)
    {
        builder.ToTable("external_game_records");

        builder.HasKey(record => record.Id);

        builder.Property(record => record.Id)
            .HasColumnName("id");

        builder.Property(record => record.DataSourceId)
            .HasColumnName("data_source_id")
            .IsRequired();

        builder.Property(record => record.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(ExternalGameRecord.MaxExternalIdLength)
            .IsRequired();

        builder.Property(record => record.GameId)
            .HasColumnName("game_id");

        builder.Property(record => record.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(record => record.FirstSeenAt)
            .HasColumnName("first_seen_at")
            .IsRequired();

        builder.Property(record => record.LastSeenAt)
            .HasColumnName("last_seen_at")
            .IsRequired();

        builder.Property(record => record.SourceUpdatedAt)
            .HasColumnName("source_updated_at");

        builder.HasIndex(
                record => new
                {
                    record.DataSourceId,
                    record.ExternalId
                })
            .IsUnique()
            .HasDatabaseName(
                "ux_external_game_records_source_external_id");

        builder.HasIndex(record => record.GameId)
            .HasDatabaseName(
                "ix_external_game_records_game_id");

        builder.HasOne<DataSource>()
            .WithMany()
            .HasForeignKey(record => record.DataSourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Game>()
            .WithMany()
            .HasForeignKey(record => record.GameId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}