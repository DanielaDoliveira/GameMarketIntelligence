using GameMarketIntel.Application.Games.Search;
using GameMarketIntel.Shared.Contracts.Games;
using GameMarketIntel.Shared.Contracts.Games.Search;
using Microsoft.AspNetCore.Mvc;

namespace GameMarketIntel.Api.Documentation;

public static class GameEndpointDocumentation
{
    public static RouteHandlerBuilder WithSearchGamesDocumentation(this RouteHandlerBuilder builder)
    {
        return builder
            .WithName("SearchGames")
            .WithTags("Games")
            .WithSummary("Searches games")
            .WithDescription(
                """
                Searches games using optional name, genre, platform,
                and release year filters.

                Results are paginated. When pagination parameters are not
                provided, the first page is returned with the default page size.
                """)
            .Produces<SearchGamesResult>( StatusCodes.Status200OK)
            .Produces<ValidationProblemDetails>( StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>( StatusCodes.Status500InternalServerError);
    }

    public static RouteHandlerBuilder WithGetGameByIdDocumentation(this RouteHandlerBuilder builder)
    {
        return builder
            .WithName("GetGameById")
            .WithTags("Games")
            .WithSummary("Gets a game by ID")
            .WithDescription(
                """
                Returns the details of a single game identified by its unique ID.

                The response includes the game's general information and its
                associated genres and platforms.

                Returns 404 when no game exists with the supplied ID.
                """)
            .Produces<GameDetails>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>( StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}