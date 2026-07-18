using System.Net;
using System.Net.Http.Json;
using GameMarketIntel.Application.Abstractions.Services;
using GameMarketIntel.Shared.Contracts.Games;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Shouldly;

namespace GameMarketIntel.Api.Tests.Endpoints;

public sealed class GamesEndpointsTests
{
    [Fact]
    public async Task GetGameById_ShouldReturnOkWithGameDetails()
    {
        // Arrange
        var gameId = Guid.NewGuid();

        var expectedGame = new GameDetails(
            gameId,
            "Celeste",
            "A narrative platform game.",
            new DateOnly(2018, 1, 25),
            "https://example.com/celeste.png",
            [],
            []);

        var gameService = Substitute.For<IGameService>();

        gameService
            .GetByIdAsync(gameId, Arg.Any<CancellationToken>())
            .Returns(expectedGame);

        await using var factory =new GameMarketIntelApiFactory(gameService);

        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync( $"/api/games/{gameId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<GameDetails>();

        result.ShouldNotBeNull();
        result.Id.ShouldBe(gameId);
        result.Name.ShouldBe("Celeste");
        result.Description.ShouldBe( "A narrative platform game.");
        result.ReleaseDate.ShouldBe( new DateOnly(2018, 1, 25));
        result.ImageUrl.ShouldBe( "https://example.com/celeste.png");

        await gameService.Received(1)
            .GetByIdAsync(  gameId, Arg.Any<CancellationToken>());
    }

    private sealed class GameMarketIntelApiFactory(IGameService gameService) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting(
            "ConnectionStrings:DefaultConnection",
            """
                Host=localhost;
                Database=gamemarketintel-tests;
                Username=postgres;
                Password=postgres
                """);


            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IGameService>();

                services.AddScoped(_ => gameService);
            });
        }
    }
}