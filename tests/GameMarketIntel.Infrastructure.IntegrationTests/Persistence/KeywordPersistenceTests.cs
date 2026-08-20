using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence;

[Collection(PostgreSqlCollection.Name)]
public sealed class KeywordPersistenceTests
{
    private readonly PostgreSqlFixture _fixture;

    public KeywordPersistenceTests(PostgreSqlFixture fixture)=>_fixture = fixture;

    [Fact]
    public async Task SaveAndLoad_ShouldPersistKeyword()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var keyword = new Keyword("Time loop");

        // Act
        await using (var writeDbContext = _fixture.CreateDbContext())
        {
            writeDbContext.Keywords.Add(keyword);
            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = _fixture.CreateDbContext();
        var persistedKeyword = await readDbContext.Keywords.SingleAsync();

        // Assert
        persistedKeyword.Id.ShouldBe(keyword.Id);
        persistedKeyword.Name.ShouldBe("Time loop");
        persistedKeyword.NormalizedName.ShouldBe("TIME LOOP");
    }

    [Fact]
    public async Task SaveChanges_ShouldRejectDuplicateNormalizedName()
    {
        await _fixture.ResetDatabaseAsync();

        // Arrange
        var firstKeyword = new Keyword("Memory loss");
        var duplicateKeyword = new Keyword("mEmOrY lOsS");

        await using var dbContext = _fixture.CreateDbContext();
        dbContext.Keywords.AddRange(firstKeyword, duplicateKeyword);

        // Act
        var action = () => dbContext.SaveChangesAsync();

        // Assert
        await action.ShouldThrowAsync<DbUpdateException>();
    }
}