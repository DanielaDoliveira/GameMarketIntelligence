using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMarketIntel.Infrastructure.Persistence.Configurations;

public sealed class GameCompanyConfiguration : IEntityTypeConfiguration<GameCompany>
{
    public void Configure(EntityTypeBuilder<GameCompany> builder)
    {
        builder.ToTable("game_companies");

        builder.HasKey(
            gameCompany => new
            {
                gameCompany.ExternalGameRecordId,
                gameCompany.ExternalCompanyRecordId,
                gameCompany.Role
            });

        builder.Property(gameCompany => gameCompany.GameId)
            .HasColumnName("game_id")
            .IsRequired();

        builder.Property(gameCompany => gameCompany.CompanyId)
            .HasColumnName("company_id")
            .IsRequired();

        builder.Property(gameCompany => gameCompany.ExternalGameRecordId)
            .HasColumnName("external_game_record_id")
            .IsRequired();

        builder.Property(gameCompany => gameCompany.ExternalCompanyRecordId)
            .HasColumnName("external_company_record_id")
            .IsRequired();

        builder.Property(gameCompany => gameCompany.Role)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnName("role")
            .IsRequired();

        builder.HasOne<Game>()
            .WithMany()
            .HasForeignKey(gameCompany => gameCompany.GameId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(gameCompany => gameCompany.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ExternalGameRecord>()
            .WithMany()
            .HasForeignKey(gameCompany => gameCompany.ExternalGameRecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ExternalCompanyRecord>()
            .WithMany()
            .HasForeignKey(gameCompany => gameCompany.ExternalCompanyRecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(gameCompany => gameCompany.GameId)
            .HasDatabaseName("ix_game_companies_game_id");

        builder.HasIndex(gameCompany => gameCompany.CompanyId)
            .HasDatabaseName("ix_game_companies_company_id");
    }
}