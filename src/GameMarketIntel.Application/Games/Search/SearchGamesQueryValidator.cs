using FluentValidation;

namespace GameMarketIntel.Application.Games.Search;

public sealed class SearchGamesQueryValidator : AbstractValidator<SearchGamesQuery>
{
    private const int MaximumPageSize = 100;

    public SearchGamesQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be greater than or equal to 1.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, MaximumPageSize)
            .WithMessage(  $"Page size must be between 1 and {MaximumPageSize}.");

        RuleFor(query => query.ReleaseYear)
            .LessThanOrEqualTo(DateTime.UtcNow.Year)
            .When(query => query.ReleaseYear.HasValue)
            .WithMessage($"Release year must be less than or equal to {DateTime.UtcNow.Year}.");
    }
}