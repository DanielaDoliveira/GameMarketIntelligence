using GameMarketIntel.Api.Documentation;
using GameMarketIntel.Application.Abstractions.Services;
using GameMarketIntel.Shared.Contracts.Platforms;

namespace GameMarketIntel.Api.Endpoints;

public static class PlatformEndpoints
{
    public static IEndpointRouteBuilder MapPlatformEndpoints( this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/platforms",async ( IPlatformService platformService,CancellationToken cancellationToken) =>
                {
                    IReadOnlyList<PlatformDetails> platforms =  await platformService.GetAllAsync(cancellationToken);

                    return Results.Ok(platforms);
                })
            .WithGetAllPlatformsDocumentation();

        return endpoints;
    }
}