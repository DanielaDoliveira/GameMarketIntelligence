using GameMarketIntel.Shared.Contracts.Genres;

namespace GameMarketIntel.Api.Documentation;

public static class GenreEndpointDocumentation
{
    public static RouteHandlerBuilder WithGetAllGenresDocumentation(this RouteHandlerBuilder builder)
    {
        return builder
            .WithName("GetAllGenres")
            .WithTags("Genres")
            .WithSummary("Gets all genres")
            .WithDescription(
                """
                Returns all available genres ordered by name.

                This endpoint is intended to populate genre filters in the client application.
                """)
            .Produces<IReadOnlyList<GenreDetails>>(StatusCodes.Status200OK);
    }
}