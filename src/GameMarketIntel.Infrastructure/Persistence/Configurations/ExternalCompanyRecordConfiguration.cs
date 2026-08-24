using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMarketIntel.Infrastructure.Persistence.Configurations;

public sealed class ExternalCompanyRecordConfiguration : IEntityTypeConfiguration<ExternalCompanyRecord>
{
    public void Configure(EntityTypeBuilder<ExternalCompanyRecord> builder)
    {
        builder.ToTable("external_company_records");

        builder.HasKey(record => record.Id);

        builder.Property(record => record.Id)
            .HasColumnName("id");

        builder.Property(record => record.DataSourceId)
            .HasColumnName("data_source_id")
            .IsRequired();

        builder.Property(record => record.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(ExternalCompanyRecord.MaxExternalIdLength)
            .IsRequired();

        builder.Property(record => record.CompanyId)
            .HasColumnName("company_id");

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

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(record => record.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(record => new { record.DataSourceId, record.ExternalId })
            .IsUnique()
            .HasDatabaseName("ux_external_company_records_source_external_id");

        builder.HasIndex(record => record.CompanyId)
            .HasDatabaseName("ix_external_company_records_company_id");
    }
}