using GameMarketIntel.Shared.Contracts.Games.Search;
using GameMarketIntel.Shared.Contracts.Genres;
using GameMarketIntel.Shared.Contracts.Platforms;
using GameMarketIntel.Shared.Requests.Games;
using GameMarketIntel.Web.Services;
using Microsoft.AspNetCore.Components;

namespace GameMarketIntel.Web.Pages;

public partial class Games
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;

    [Inject]
    private IGameApiService GameApiService { get; set; } = default!;

    [Inject]
    private IGenreApiService GenreApiService { get; set; } = default!;

    [Inject]
    private IPlatformApiService PlatformApiService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter]
    [SupplyParameterFromQuery(Name = "search")]
    public string? Search { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "genreId")]
    public Guid? GenreId { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "platformId")]
    public Guid? PlatformId { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "releaseYear")]
    public int? ReleaseYear { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "page")]
    public int? Page { get; set; }

    private readonly int CurrentYear =
        DateTime.UtcNow.Year;

    private IReadOnlyList<GenreDetails> _genres = [];

    private IReadOnlyList<PlatformDetails> _platforms = [];

    private SearchGamesResult? _gamesResult;

    private string? _searchTerm;

    private string? _lastAppliedSearch;

    private Guid? _selectedGenreId;

    private Guid? _selectedPlatformId;

    private int? _releaseYear;

    private int _currentPage = DefaultPage;

    private bool _pageInitialized;

    private bool _isInitialLoading = true;

    private bool _isLoading;

    private bool _referenceDataLoaded;

    private bool _hasCompletedUnfilteredSearch;

    private string? _errorMessage;

    private bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(_searchTerm) ||
        _selectedGenreId.HasValue ||
        _selectedPlatformId.HasValue ||
        _releaseYear.HasValue;

    private bool HasAnyGameData =>
        !_hasCompletedUnfilteredSearch ||
        (_gamesResult?.TotalItems ?? 0) > 0;

    private string ResultsStatusText
    {
        get
        {
            if (_isInitialLoading)
            {
                return "Loading comparable games.";
            }

            if (_isLoading)
            {
                return "Updating results.";
            }

            if (_errorMessage is not null)
            {
                return
                    "Comparable games could not be loaded.";
            }

            var totalItems =
                _gamesResult?.TotalItems ?? 0;

            return totalItems switch
            {
                0 => "No games found.",
                1 => "1 game found.",
                _ => $"{totalItems} games found."
            };
        }
    }

    private string ResultsGridCssClass =>
        _isLoading
            ? "results-grid results-grid--loading"
            : "results-grid";

    protected override async Task OnParametersSetAsync()
    {
        var normalizedSearch =
            NormalizeSearch(Search);

        var normalizedReleaseYear =
            IsValidReleaseYear(ReleaseYear)
                ? ReleaseYear
                : null;

        var normalizedPage =
            Page is > 0
                ? Page.Value
                : DefaultPage;

        var queryStateChanged =
            HasQueryStateChanged(
                normalizedSearch,
                GenreId,
                PlatformId,
                normalizedReleaseYear,
                normalizedPage);

        if (!_pageInitialized)
        {
            _pageInitialized = true;

            ApplyQueryState(
                normalizedSearch,
                GenreId,
                PlatformId,
                normalizedReleaseYear,
                normalizedPage);

            await LoadInitialDataAsync();

            return;
        }

        if (!queryStateChanged)
        {
            return;
        }

        ApplyQueryState(
            normalizedSearch,
            GenreId,
            PlatformId,
            normalizedReleaseYear,
            normalizedPage);

        await RefreshResultsAsync();
    }

    private bool HasQueryStateChanged(
        string? search,
        Guid? genreId,
        Guid? platformId,
        int? releaseYear,
        int page)
    {
        return
            !string.Equals(
                _lastAppliedSearch,
                search,
                StringComparison.OrdinalIgnoreCase) ||
            _selectedGenreId != genreId ||
            _selectedPlatformId != platformId ||
            _releaseYear != releaseYear ||
            _currentPage != page;
    }

    private void ApplyQueryState(
        string? search,
        Guid? genreId,
        Guid? platformId,
        int? releaseYear,
        int page)
    {
        _searchTerm = search;
        _lastAppliedSearch = search;
        _selectedGenreId = genreId;
        _selectedPlatformId = platformId;
        _releaseYear = releaseYear;
        _currentPage = page;
    }

    private async Task LoadInitialDataAsync()
    {
        _isInitialLoading = true;
        _errorMessage = null;

        try
        {
            var genresTask =
                GenreApiService.GetAllAsync();

            var platformsTask =
                PlatformApiService.GetAllAsync();

            var gamesTask =
                SearchGamesAsync();

            await Task.WhenAll(
                genresTask,
                platformsTask,
                gamesTask);

            _genres =
                await genresTask;

            _platforms =
                await platformsTask;

            _gamesResult =
                await gamesTask;

            _referenceDataLoaded = true;
            _hasCompletedUnfilteredSearch = true;
        }
        catch (HttpRequestException)
        {
            SetServiceUnavailableError();
        }
        finally
        {
            _isInitialLoading = false;
        }
    }

    private async Task RefreshResultsAsync()
    {
        _isLoading = true;
        _errorMessage = null;

        try
        {
            _gamesResult =
                await SearchGamesAsync();
        }
        catch (HttpRequestException)
        {
            SetServiceUnavailableError();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private Task<SearchGamesResult> SearchGamesAsync()
    {
        var request =
            new SearchGamesRequest
            {
                Search = _searchTerm,
                GenreId = _selectedGenreId,
                PlatformId = _selectedPlatformId,
                ReleaseYear = _releaseYear,
                Page = _currentPage,
                PageSize = DefaultPageSize
            };

        return GameApiService.SearchAsync(request);
    }

    private void HandleGenreChanged(
        ChangeEventArgs args)
    {
        var genreId =
            TryParseGuid(
                args.Value?.ToString());

        NavigateWithQueryChanges(
            ("genreId", genreId?.ToString()),
            ("page", null));
    }

    private void HandlePlatformChanged(
        ChangeEventArgs args)
    {
        var platformId =
            TryParseGuid(
                args.Value?.ToString());

        NavigateWithQueryChanges(
            ("platformId", platformId?.ToString()),
            ("page", null));
    }

    private void HandleReleaseYearChanged(
        ChangeEventArgs args)
    {
        var releaseYear =
            TryParseReleaseYear(
                args.Value?.ToString());

        NavigateWithQueryChanges(
            ("releaseYear", releaseYear?.ToString()),
            ("page", null));
    }

    private void ClearFilters()
    {
        NavigationManager.NavigateTo("/games");
    }

    private void GoToPreviousPage()
    {
        if (_isLoading ||
            _gamesResult is null ||
            _gamesResult.Page <= DefaultPage)
        {
            return;
        }

        var previousPage =
            _gamesResult.Page - 1;

        NavigateWithQueryChanges(
            (
                "page",
                previousPage == DefaultPage
                    ? null
                    : previousPage.ToString()
            ));
    }

    private void GoToNextPage()
    {
        if (_isLoading ||
            _gamesResult is null ||
            _gamesResult.Page >=
            _gamesResult.TotalPages)
        {
            return;
        }

        NavigateWithQueryChanges(
            (
                "page",
                (_gamesResult.Page + 1).ToString()
            ));
    }

    private async Task RetryAsync()
    {
        if (!_referenceDataLoaded)
        {
            await LoadInitialDataAsync();

            return;
        }

        await RefreshResultsAsync();
    }

    private void RemoveSearchFilter()
    {
        RemoveFilter("search");
    }

    private void RemoveGenreFilter()
    {
        RemoveFilter("genreId");
    }

    private void RemovePlatformFilter()
    {
        RemoveFilter("platformId");
    }

    private void RemoveReleaseYearFilter()
    {
        RemoveFilter("releaseYear");
    }

    private void RemoveFilter(
        string parameterName)
    {
        NavigateWithQueryChanges(
            (parameterName, null),
            ("page", null));
    }

    private void NavigateWithQueryChanges(
        params (string Name, string? Value)[] changes)
    {
        var currentUri =
            NavigationManager.ToAbsoluteUri(
                NavigationManager.Uri);

        var query =
            System.Web.HttpUtility.ParseQueryString(
                currentUri.Query);

        foreach (var change in changes)
        {
            if (string.IsNullOrWhiteSpace(change.Value))
            {
                query.Remove(change.Name);
            }
            else
            {
                query[change.Name] = change.Value;
            }
        }

        var queryString =
            query.ToString();

        var targetUri =
            string.IsNullOrWhiteSpace(queryString)
                ? currentUri.AbsolutePath
                : $"{currentUri.AbsolutePath}?{queryString}";

        NavigationManager.NavigateTo(targetUri);
    }

    private void SetServiceUnavailableError()
    {
        _errorMessage =
            "Please try again. " +
            "The research service may still be starting.";
    }

    private bool IsValidReleaseYear(
        int? releaseYear)
    {
        return releaseYear is >= 1950 &&
               releaseYear <= CurrentYear;
    }

    private int? TryParseReleaseYear(
        string? value)
    {
        if (!int.TryParse(
                value,
                out var year))
        {
            return null;
        }

        return IsValidReleaseYear(year)
            ? year
            : null;
    }

    private static Guid? TryParseGuid(
        string? value)
    {
        return Guid.TryParse(
            value,
            out var id)
                ? id
                : null;
    }

    private static string? NormalizeSearch(
        string? search)
    {
        return string.IsNullOrWhiteSpace(search)
            ? null
            : search.Trim();
    }
}