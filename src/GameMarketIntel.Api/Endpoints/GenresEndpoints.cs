using GameMarketIntel.Api.Documentation;
using GameMarketIntel.Application.Abstractions.Services;
using GameMarketIntel.Shared.Contracts.Genres;

namespace GameMarketIntel.Api.Endpoints;

public static class GenreEndpoints
{
    public static IEndpointRouteBuilder MapGenreEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet( "/api/genres",
                async (IGenreService genreService,CancellationToken cancellationToken) =>
                {
                    IReadOnlyList<GenreDetails> genres =await genreService.GetAllAsync(cancellationToken);

                    return Results.Ok(genres);
                })
            .WithGetAllGenresDocumentation();

        return endpoints;
    }
}