using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMarketIntel.Infrastructure.Persistence.Configurations;

public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("companies");

        builder.HasKey(company => company.Id);

        builder.Property(company => company.Id)
            .HasColumnName("id");

        builder.Property(company => company.Name)
            .HasColumnName("name")
            .HasMaxLength(Company.MaxNameLength)
            .IsRequired();

        builder.Property(company => company.NormalizedName)
            .HasColumnName("normalized_name")
            .HasMaxLength(Company.MaxNameLength)
            .IsRequired();

        builder.HasIndex(company => company.NormalizedName)
            .IsUnique()
            .HasDatabaseName("ux_companies_normalized_name");
    }
}