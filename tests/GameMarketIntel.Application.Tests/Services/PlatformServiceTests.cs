using GameMarketIntel.Application.Abstractions.Persistence;
using GameMarketIntel.Application.Services;
using GameMarketIntel.Domain.Entities;
using NSubstitute;
using Shouldly;

namespace GameMarketIntel.Application.Tests.Services;

public sealed class PlatformServiceTests
{
    [Fact]
    public async Task GetAllAsync_ShouldMapPlatformsToDetails()
    {
        // Arrange
        var pc = new Platform(
            name: "PC",
            family: "Computer",
            manufacturer: null,
            imageUrl: "https://example.com/pc.png");

        var playStation5 = new Platform(
            name: "PlayStation 5",
            family: "PlayStation",
            manufacturer: "Sony",
            imageUrl: "https://example.com/ps5.png");

        var repository = Substitute.For<IPlatformRepository>();

        repository
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([pc, playStation5]);

        var service = new PlatformService(repository);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Count.ShouldBe(2);

        result[0].Id.ShouldBe(pc.Id);
        result[0].Name.ShouldBe("PC");
        result[0].Family.ShouldBe("Computer");
        result[0].Manufacturer.ShouldBeNull();
        result[0].ImageUrl.ShouldBe("https://example.com/pc.png");

        result[1].Id.ShouldBe(playStation5.Id);
        result[1].Name.ShouldBe("PlayStation 5");
        result[1].Family.ShouldBe("PlayStation");
        result[1].Manufacturer.ShouldBe("Sony");
        result[1].ImageUrl.ShouldBe("https://example.com/ps5.png");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoPlatformsExist()
    {
        // Arrange
        var repository = Substitute.For<IPlatformRepository>();

        repository
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Platform>());

        var service = new PlatformService(repository);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldPassCancellationTokenToRepository()
    {
        // Arrange
        var repository = Substitute.For<IPlatformRepository>();

        repository
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Platform>());

        var service = new PlatformService(repository);

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