using GameMarketIntel.Shared.Contracts.Platforms;

namespace GameMarketIntel.Api.Documentation;

public static class PlatformEndpointDocumentation
{
    public static RouteHandlerBuilder WithGetAllPlatformsDocumentation(this RouteHandlerBuilder builder)
    {
        return builder
            .WithName("GetAllPlatforms")
            .WithTags("Platforms")
            .WithSummary("Gets all platforms")
            .WithDescription(
                """
                Returns all available platforms ordered by name.

                This endpoint is intended to populate platform filters
                in the client application.
                """)
            .Produces<IReadOnlyList<PlatformDetails>>(StatusCodes.Status200OK);
    }
}