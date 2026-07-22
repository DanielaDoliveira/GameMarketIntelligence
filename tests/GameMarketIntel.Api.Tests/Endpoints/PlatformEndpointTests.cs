using System.Net;
using System.Net.Http.Json;
using GameMarketIntel.Application.Abstractions.Services;
using GameMarketIntel.Shared.Contracts.Platforms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Shouldly;

namespace GameMarketIntel.Api.Tests.Endpoints;

public sealed class PlatformEndpointsTests
{
    [Fact]
    public async Task GetAllPlatforms_ShouldReturnOkWithPlatforms()
    {
        // Arrange
        IReadOnlyList<PlatformDetails> expectedPlatforms =
        [
            new PlatformDetails(
                Guid.NewGuid(),
                "Nintendo Switch",
                "Nintendo Switch",
                "Nintendo",
                "https://example.com/switch.png"),
            new PlatformDetails(
                Guid.NewGuid(),
                "PlayStation 5",
                "PlayStation",
                "Sony",
                "https://example.com/ps5.png")
        ];

        var platformService = Substitute.For<IPlatformService>();

        platformService
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(expectedPlatforms);

        await using var factory =
            new GameMarketIntelApiFactory(platformService);

        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/platforms");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content
            .ReadFromJsonAsync<IReadOnlyList<PlatformDetails>>();

        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);

        result[0].Id.ShouldBe(expectedPlatforms[0].Id);
        result[0].Name.ShouldBe("Nintendo Switch");
        result[0].Family.ShouldBe("Nintendo Switch");
        result[0].Manufacturer.ShouldBe("Nintendo");
        result[0].ImageUrl.ShouldBe(
            "https://example.com/switch.png");

        result[1].Id.ShouldBe(expectedPlatforms[1].Id);
        result[1].Name.ShouldBe("PlayStation 5");
        result[1].Family.ShouldBe("PlayStation");
        result[1].Manufacturer.ShouldBe("Sony");
        result[1].ImageUrl.ShouldBe(
            "https://example.com/ps5.png");

        await platformService
            .Received(1)
            .GetAllAsync(Arg.Any<CancellationToken>());
    }

    private sealed class GameMarketIntelApiFactory(
        IPlatformService platformService)
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
                services.RemoveAll<IPlatformService>();
                services.AddScoped(_ => platformService);
            });
        }
    }
}