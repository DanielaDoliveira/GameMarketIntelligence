using GameMarketIntel.Application.Games.Search;

namespace GameMarketIntel.Api.Documentation;

public static class GameEndpointDocumentation
{
    public static RouteHandlerBuilder WithSearchGamesDocumentation(
        this RouteHandlerBuilder builder)
    {
        return builder
            .WithName("SearchGames")
            .WithTags("Games")
            .WithSummary("Searches games")
            .WithDescription(
                """
                Searches games using optional name, genre, platform, and
                release-year filters.

                Results are ordered alphabetically by game name and returned
                with pagination.

                When no filters are provided, all games are listed.
                """)
            .Produces<SearchGamesResult>(
                StatusCodes.Status200OK);
    }
}