using GameMarketIntel.Application.Abstractions.Persistence;
using GameMarketIntel.Application.Services;
using GameMarketIntel.Domain.Entities;
using NSubstitute;
using Shouldly;

namespace GameMarketIntel.Application.Tests.Services;

public sealed class GenreServiceTests
{
    [Fact]
    public async Task GetAllAsync_ShouldMapGenresToDetails()
    {
        // Arrange
        var action = new Genre("Action");
        var adventure = new Genre("Adventure");

        var repository = Substitute.For<IGenreRepository>();

        repository
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([action, adventure]);

        var service = new GenreService(repository);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Count.ShouldBe(2);

        result[0].Id.ShouldBe(action.Id);
        result[0].Name.ShouldBe("Action");

        result[1].Id.ShouldBe(adventure.Id);
        result[1].Name.ShouldBe("Adventure");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoGenresExist()
    {
        // Arrange
        var repository = Substitute.For<IGenreRepository>();

        repository
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Genre>());

        var service = new GenreService(repository);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldPassCancellationTokenToRepository()
    {
        // Arrange
        var repository = Substitute.For<IGenreRepository>();

        repository
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Genre>());

        var service = new GenreService(repository);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken = cancellationTokenSource.Token;

        // Act
        await service.GetAllAsync(cancellationToken);

        // Assert
        await repository
            .Received(1)
            .GetAllAsync(cancellationToken);
    }


}