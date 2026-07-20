using System.Net;
using System.Net.Http.Json;
using GameMarketIntel.Application.Abstractions.Services;
using GameMarketIntel.Shared.Contracts.Genres;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Shouldly;

namespace GameMarketIntel.Api.Tests.Endpoints;

public sealed class GenreEndpointsTests
{
    [Fact]
    public async Task GetAllGenres_ShouldReturnOkWithGenres()
    {
        // Arrange
        IReadOnlyList<GenreDetails> expectedGenres =
        [
            new GenreDetails(
                Guid.NewGuid(),
                "Action"),
            new GenreDetails(
                Guid.NewGuid(),
                "Adventure")
        ];

        var genreService = Substitute.For<IGenreService>();

        genreService
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(expectedGenres);

        await using var factory =
            new GameMarketIntelApiFactory(genreService);

        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/genres");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content
            .ReadFromJsonAsync<IReadOnlyList<GenreDetails>>();

        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);

        result[0].Id.ShouldBe(expectedGenres[0].Id);
        result[0].Name.ShouldBe("Action");

        result[1].Id.ShouldBe(expectedGenres[1].Id);
        result[1].Name.ShouldBe("Adventure");

        await genreService
            .Received(1)
            .GetAllAsync(Arg.Any<CancellationToken>());
    }

    private sealed class GameMarketIntelApiFactory(
        IGenreService genreService)
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(
            IWebHostBuilder builder)
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
                services.RemoveAll<IGenreService>();

                services.AddScoped(_ => genreService);
            });
        }
    }
}