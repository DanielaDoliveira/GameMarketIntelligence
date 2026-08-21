using GameMarketIntel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameMarketIntel.Infrastructure.Persistence.Configurations;

public sealed class GameKeywordConfiguration : IEntityTypeConfiguration<GameKeyword>
{
    public void Configure(EntityTypeBuilder<GameKeyword> builder)
    {
        builder.ToTable("game_keywords");

        builder.HasKey(gameKeyword => new
        {
            gameKeyword.ExternalGameRecordId,
            gameKeyword.ExternalKeywordRecordId
        });

        builder.Property(gameKeyword => gameKeyword.GameId)
            .HasColumnName("game_id");

        builder.Property(gameKeyword => gameKeyword.KeywordId)
            .HasColumnName("keyword_id");

        builder.Property(gameKeyword => gameKeyword.ExternalGameRecordId)
            .HasColumnName("external_game_record_id");

        builder.Property(gameKeyword => gameKeyword.ExternalKeywordRecordId)
            .HasColumnName("external_keyword_record_id");

        builder.HasOne<Game>()
            .WithMany()
            .HasForeignKey(gameKeyword => gameKeyword.GameId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Keyword>()
            .WithMany()
            .HasForeignKey(gameKeyword => gameKeyword.KeywordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ExternalGameRecord>()
            .WithMany()
            .HasForeignKey(gameKeyword => gameKeyword.ExternalGameRecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ExternalKeywordRecord>()
            .WithMany()
            .HasForeignKey(gameKeyword => gameKeyword.ExternalKeywordRecordId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}