using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace GameMarketIntel.Infrastructure.Persistence.Configurations;

public sealed class DataSourceConfiguration: IEntityTypeConfiguration<DataSource>
{
    public void Configure(EntityTypeBuilder<DataSource> builder)
    {
        builder.ToTable("data_sources");
        builder.HasKey(source => source.Id);
        builder.Property(source => source.Id).HasColumnName("id");

        builder.Property(source => source.Name)
           .HasColumnName("name")
           .HasMaxLength(150)
           .IsRequired();

        builder.Property(source => source.Url)
                  .HasColumnName("url")
                  .HasMaxLength(500)
                  .IsRequired();

        builder.Property(source => source.LicenseNotes)
          .HasColumnName("license_notes")
          .HasMaxLength(2000);

        builder.Property(source => source.AttributionRequired)
       .HasColumnName("attribution_required")
       .IsRequired();

        builder.OwnsOne(
           source => source.Reliability,
           reliability =>
           {
               reliability.Property(value => value.Level)
                   .HasColumnName("reliability_level")
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

               reliability.Property(value => value.Reason)
                   .HasColumnName("reliability_reason")
                   .HasMaxLength(500)
                   .IsRequired();

               reliability.Property(value => value.Limitations)
                   .HasColumnName("reliability_limitations")
                   .HasMaxLength(2000);
           });

    }
}
