using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMarketIntel.Infrastructure.Persistence.Configurations;

public sealed class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.HasKey(game => game.Id);

        builder.Property(game => game.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(game => game.NormalizedName)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(game => game.NormalizedName);

        builder.Property(game => game.Description)
            .HasMaxLength(4000);

        builder.Property(game => game.FirstReleaseDate);

        builder.Property(game => game.ImageUrl)
            .HasMaxLength(2048);

        builder
            .HasMany(game => game.Genres)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "GameGenre",
                right => right
                    .HasOne<Genre>()
                    .WithMany()
                    .HasForeignKey("GenreId")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<Game>()
                    .WithMany()
                    .HasForeignKey("GameId")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("GameGenres");
                    join.HasKey("GameId", "GenreId");
                });

        builder
            .HasMany(game => game.Platforms)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "GamePlatform",
                right => right
                    .HasOne<Platform>()
                    .WithMany()
                    .HasForeignKey("PlatformId")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<Game>()
                    .WithMany()
                    .HasForeignKey("GameId")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("GamePlatforms");
                    join.HasKey("GameId", "PlatformId");
                });
    }
}