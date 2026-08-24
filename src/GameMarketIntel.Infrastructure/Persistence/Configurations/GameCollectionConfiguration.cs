using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMarketIntel.Infrastructure.Persistence.Configurations;

public sealed class GameCollectionConfiguration : IEntityTypeConfiguration<GameCollection>
{
    public void Configure(EntityTypeBuilder<GameCollection> builder)
    {
        builder.ToTable("game_collections");

        builder.HasKey(
            gameCollection => new
            {
                gameCollection.ExternalGameRecordId,
                gameCollection.ExternalCollectionRecordId
            });

        builder.Property(gameCollection => gameCollection.GameId)
            .HasColumnName("game_id")
            .IsRequired();

        builder.Property(gameCollection => gameCollection.CollectionId)
            .HasColumnName("collection_id")
            .IsRequired();

        builder.Property(gameCollection => gameCollection.ExternalGameRecordId)
            .HasColumnName("external_game_record_id")
            .IsRequired();

        builder.Property(gameCollection => gameCollection.ExternalCollectionRecordId)
            .HasColumnName("external_collection_record_id")
            .IsRequired();

        builder.HasOne<Game>()
            .WithMany()
            .HasForeignKey(gameCollection => gameCollection.GameId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Collection>()
            .WithMany()
            .HasForeignKey(gameCollection => gameCollection.CollectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ExternalGameRecord>()
            .WithMany()
            .HasForeignKey(gameCollection => gameCollection.ExternalGameRecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ExternalCollectionRecord>()
            .WithMany()
            .HasForeignKey(gameCollection => gameCollection.ExternalCollectionRecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(gameCollection => gameCollection.GameId)
            .HasDatabaseName("ix_game_collections_game_id");

        builder.HasIndex(gameCollection => gameCollection.CollectionId)
            .HasDatabaseName("ix_game_collections_collection_id");
    }
}