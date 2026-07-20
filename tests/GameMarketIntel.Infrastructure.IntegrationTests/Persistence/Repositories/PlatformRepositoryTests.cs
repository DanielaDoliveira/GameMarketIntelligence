using GameMarketIntel.Domain.Entities;
using GameMarketIntel.Infrastructure.IntegrationTests.Fixtures;
using GameMarketIntel.Infrastructure.Persistence.Repositories;
using Shouldly;

namespace GameMarketIntel.Infrastructure.IntegrationTests.Persistence.Repositories;

[Collection(PostgreSqlCollection.Name)]
public sealed class PlatformRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;

    public PlatformRepositoryTests(PostgreSqlFixture fixture)
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
    public async Task GetAllAsync_ShouldReturnPlatformsOrderedByName()
    {
        // Arrange
        await using (var arrangeDbContext = _fixture.CreateDbContext())
        {
            arrangeDbContext.Platforms.AddRange(
                new Platform(
                    name: "PlayStation 5",
                    family: "PlayStation",
                    manufacturer: "Sony",
                    imageUrl: null),
                new Platform(
                    name: "Nintendo Switch",
                    family: "Nintendo Switch",
                    manufacturer: "Nintendo",
                    imageUrl: null),
                new Platform(
                    name: "PC",
                    family: "Computer",
                    manufacturer: null,
                    imageUrl: null));

            await arrangeDbContext.SaveChangesAsync();
        }

        await using var queryDbContext = _fixture.CreateDbContext();

        var repository = new PlatformRepository(queryDbContext);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Count.ShouldBe(3);

        result
            .Select(platform => platform.Name)
            .ShouldBe(
                [
                    "Nintendo Switch",
                    "PC",
                    "PlayStation 5"
                ],
                ignoreOrder: false);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoPlatformsExist()
    {
        // Arrange
        await using var queryDbContext = _fixture.CreateDbContext();

        var repository = new PlatformRepository(queryDbContext);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.ShouldBeEmpty();
    }
}