using GameMarketIntel.Application.Games.Search;
using Shouldly;

namespace GameMarketIntel.Application.Tests.Games.Search;

public sealed class SearchGamesQueryValidatorTests
{
    private readonly SearchGamesQueryValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_ShouldBeValid_WhenQueryIsValid()
    {
        // Arrange
        var query = new SearchGamesQuery(
            Search: "Hades",
            GenreId: null,
            PlatformId: null,
            ReleaseYear: DateTime.UtcNow.Year,
            Page: 1,
            PageSize: 20);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_ShouldFail_WhenPageIsLessThanOne()
    {
        // Arrange
        var query = new SearchGamesQuery(
            Search: null,
            GenreId: null,
            PlatformId: null,
            ReleaseYear: null,
            Page: 0,
            PageSize: 20);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.ShouldBeFalse();

        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(SearchGamesQuery.Page));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task ValidateAsync_ShouldFail_WhenPageSizeIsOutsideAllowedRange(
        int pageSize)
    {
        // Arrange
        var query = new SearchGamesQuery(
            Search: null,
            GenreId: null,
            PlatformId: null,
            ReleaseYear: null,
            Page: 1,
            PageSize: pageSize);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.ShouldBeFalse();

        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(SearchGamesQuery.PageSize));
    }

    [Fact]
    public async Task ValidateAsync_ShouldFail_WhenReleaseYearIsInTheFuture()
    {
        // Arrange
        var query = new SearchGamesQuery(
            Search: null,
            GenreId: null,
            PlatformId: null,
            ReleaseYear: DateTime.UtcNow.Year + 1,
            Page: 1,
            PageSize: 20);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.ShouldBeFalse();

        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(SearchGamesQuery.ReleaseYear));
    }

    [Fact]
    public async Task ValidateAsync_ShouldBeValid_WhenReleaseYearIsNull()
    {
        // Arrange
        var query = new SearchGamesQuery(
            Search: null,
            GenreId: null,
            PlatformId: null,
            ReleaseYear: null,
            Page: 1,
            PageSize: 20);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }
}