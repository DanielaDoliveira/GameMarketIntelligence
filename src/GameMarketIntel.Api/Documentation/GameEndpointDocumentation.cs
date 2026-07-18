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

                Pagination rules:
                - Page must be greater than or equal to 1.
                - Page size must be between 1 and 100.
                - The default page is 1.
                - The default page size is 20.

                Release-year rules:
                - The release year is optional.
                - When provided, it cannot be greater than the current year.

                Validation failures are currently handled by the application
                validator. Standardized HTTP 400 responses will be added with
                the global exception-handling increment.
                """)
            .Produces<SearchGamesResult>(
                StatusCodes.Status200OK);
    }
}