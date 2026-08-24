using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMarketIntel.Infrastructure.Persistence.Configurations;

public sealed class GameProductRelationConfiguration : IEntityTypeConfiguration<GameProductRelation>
{
    public void Configure(EntityTypeBuilder<GameProductRelation> builder)
    {
        builder.ToTable("game_product_relations");

        builder.HasKey(relation => relation.Id);

        builder.Property(relation => relation.Id)
            .HasColumnName("id");

        builder.Property(relation => relation.SourceGameId)
            .HasColumnName("source_game_id")
            .IsRequired();

        builder.Property(relation => relation.TargetGameId)
            .HasColumnName("target_game_id")
            .IsRequired();

        builder.Property(relation => relation.ExternalSourceGameRecordId)
            .HasColumnName("external_source_game_record_id")
            .IsRequired();

        builder.Property(relation => relation.ExternalTargetGameRecordId)
            .HasColumnName("external_target_game_record_id")
            .IsRequired();

        builder.Property(relation => relation.RelationType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnName("relation_type")
            .IsRequired();

        builder.HasOne<Game>()
            .WithMany()
            .HasForeignKey(relation => relation.SourceGameId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Game>()
            .WithMany()
            .HasForeignKey(relation => relation.TargetGameId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ExternalGameRecord>()
            .WithMany()
            .HasForeignKey(relation => relation.ExternalSourceGameRecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ExternalGameRecord>()
            .WithMany()
            .HasForeignKey(relation => relation.ExternalTargetGameRecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(relation => relation.SourceGameId)
            .HasDatabaseName("ix_game_product_relations_source_game_id");

        builder.HasIndex(relation => relation.TargetGameId)
            .HasDatabaseName("ix_game_product_relations_target_game_id");

        builder.HasIndex(
                relation => new
                {
                    relation.ExternalSourceGameRecordId,
                    relation.ExternalTargetGameRecordId,
                    relation.RelationType
                })
            .IsUnique()
            .HasDatabaseName("ux_game_product_relations_external_records_type");
    }
}