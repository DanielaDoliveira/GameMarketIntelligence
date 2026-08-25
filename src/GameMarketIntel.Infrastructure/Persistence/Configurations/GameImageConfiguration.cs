using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMarketIntel.Infrastructure.Persistence.Configurations;

public sealed class GameImageConfiguration : IEntityTypeConfiguration<GameImage>
{
    public void Configure(EntityTypeBuilder<GameImage> builder)
    {
        builder.ToTable("game_images");

        builder.HasKey(image => image.Id);

        builder.Property(image => image.ExternalId)
            .HasMaxLength(GameImage.MaxExternalIdLength)
            .IsRequired();

        builder.Property(image => image.SourceImageId)
            .HasMaxLength(GameImage.MaxSourceImageIdLength)
            .IsRequired();

        builder.Property(image => image.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(image => image.GameId);

        builder.HasIndex(image => image.ExternalGameRecordId);

        builder.HasIndex(image => new
            {
                image.ExternalGameRecordId,
                image.ExternalId,
                image.Type
            })
            .IsUnique();

        builder.HasOne<Game>()
            .WithMany()
            .HasForeignKey(image => image.GameId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ExternalGameRecord>()
            .WithMany()
            .HasForeignKey(image => image.ExternalGameRecordId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}