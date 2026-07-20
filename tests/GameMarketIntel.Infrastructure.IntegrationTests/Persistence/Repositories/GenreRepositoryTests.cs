using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using GameMarketIntel.Infrastructure.Persistence.Repositories;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence.Repositories;

[Collection(PostgreSqlCollection.Name)]
public sealed class GenreRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;

    public GenreRepositoryTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync()
    {
        return _fixture.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnGenresOrderedByName()
    {
        // Arrange
        await using (var arrangeDbContext = _fixture.CreateDbContext())
        {
            arrangeDbContext.Genres.AddRange(
                new Genre("RPG"),
                new Genre("Action"),
                new Genre("Adventure"));

            await arrangeDbContext.SaveChangesAsync();
        }

        await using var queryDbContext = _fixture.CreateDbContext();

        var repository = new GenreRepository(queryDbContext);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Count.ShouldBe(3);

        result
            .Select(genre => genre.Name)
            .ShouldBe(
                [
                    "Action",
                    "Adventure",
                    "RPG"
                ],
                ignoreOrder: false);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoGenresExist()
    {
        // Arrange
        await using var queryDbContext = _fixture.CreateDbContext();

        var repository = new GenreRepository(queryDbContext);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.ShouldBeEmpty();
    }
}