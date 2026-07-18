using GameMarketIntel.Api.Documentation;
using GameMarketIntel.Application.Abstractions.Services;
using GameMarketIntel.Application.Games.Search;
namespace GameMarketIntel.Api.Endpoints;

public static class GamesEndpoints
{
    public static IEndpointRouteBuilder MapGameEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        #region Search and filter games
        endpoints.MapGet("/api/games", 
            async (
                string? search,
                Guid? genreId, 
                Guid? platformId,
                int? releaseYear, 
                int? page, 
                int? pageSize, 
                IGameSearchService gameSearchService,
                CancellationToken cancellationToken) 
                =>
            {
                var query = new SearchGamesQuery(
                    search,
                    genreId,
                    platformId,
                    releaseYear,
                    page ?? 1,
                    pageSize ?? 20);

                var result = await gameSearchService.SearchAsync( query, cancellationToken);

                return Results.Ok(result);

            })
            .WithSearchGamesDocumentation();

        #endregion

        #region Get game by ID


        endpoints.MapGet("/api/games/{id:guid}",async ( Guid id, IGameService gameService, CancellationToken cancellationToken) =>
    {
        var game = await gameService.GetByIdAsync(id, cancellationToken);
        return Results.Ok(game);

    }).WithGetGameByIdDocumentation();
       #endregion


        return endpoints;
    }



}