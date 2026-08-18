using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMarketIntel.Infrastructure.Persistence.Configurations;

public sealed class GameReleaseConfiguration
    : IEntityTypeConfiguration<GameRelease>
{
    public void Configure(
        EntityTypeBuilder<GameRelease> builder)
    {
        builder.ToTable("game_releases");

        builder.HasKey(release => release.Id);

        builder.Property(release => release.Id)
            .HasColumnName("id");

        builder.Property(release => release.GameId)
            .HasColumnName("game_id")
            .IsRequired();

        builder.Property(release => release.PlatformId)
            .HasColumnName("platform_id")
            .IsRequired();

        builder.Property(release =>
                release.ExternalGameRecordId)
            .HasColumnName("external_game_record_id")
            .IsRequired();

        builder.Property(release =>
                release.ExternalReleaseId)
            .HasColumnName("external_release_id")
            .HasMaxLength(
                GameRelease.MaximumExternalReleaseIdLength)
            .IsRequired();

        builder.Property(release => release.RegionCode)
            .HasColumnName("region_code")
            .HasMaxLength(
                GameRelease.MaximumRegionCodeLength);

        builder.Property(release => release.Status)
            .HasColumnName("status")
            .HasConversion<string>();

        builder.Property(release => release.Ecosystem)
            .HasColumnName("ecosystem")
            .HasMaxLength(
                GameRelease.MaximumEcosystemLength);

        builder.Property(release => release.ObservedAt)
            .HasColumnName("observed_at")
            .IsRequired();

        builder.OwnsOne(
            release => release.ReleaseDate,
            releaseDate =>
            {
                releaseDate.Property(value => value.Kind)
                    .HasColumnName("release_date_kind")
                    .HasConversion<string>()
                    .IsRequired();

                releaseDate.Property(value => value.Year)
                    .HasColumnName("release_year");

                releaseDate.Property(value => value.Month)
                    .HasColumnName("release_month");

                releaseDate.Property(value => value.Day)
                    .HasColumnName("release_day");

                releaseDate.Property(value => value.Quarter)
                    .HasColumnName("release_quarter");
            });

        builder.Navigation(release => release.ReleaseDate)
            .IsRequired();

        builder.HasIndex(release => release.GameId)
            .HasDatabaseName(
                "ix_game_releases_game_id");

        builder.HasIndex(release => release.PlatformId)
            .HasDatabaseName(
                "ix_game_releases_platform_id");

        builder.HasIndex(
                release => new
                {
                    release.ExternalGameRecordId,
                    release.ExternalReleaseId
                })
            .IsUnique()
            .HasDatabaseName(
                "ux_game_releases_external_record_external_release_id");

        builder.HasOne<Game>()
            .WithMany()
            .HasForeignKey(release => release.GameId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Platform>()
            .WithMany()
            .HasForeignKey(release => release.PlatformId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ExternalGameRecord>()
            .WithMany()
            .HasForeignKey(
                release => release.ExternalGameRecordId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}