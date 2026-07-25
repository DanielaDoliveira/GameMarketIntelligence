using GameMarketIntel.Shared.Contracts.Genres;
using GameMarketIntel.Shared.Contracts.Platforms;
using Microsoft.AspNetCore.Components;

namespace GameMarketIntel.Web.Components.Games;

public partial class GamesFilters
{
    [Parameter]
    public IReadOnlyList<GenreDetails> Genres { get; set; } = [];

    [Parameter]
    public IReadOnlyList<PlatformDetails> Platforms { get; set; } = [];

    [Parameter]
    public string? SearchTerm { get; set; }

    [Parameter]
    public Guid? SelectedGenreId { get; set; }

    [Parameter]
    public Guid? SelectedPlatformId { get; set; }

    [Parameter]
    public int? ReleaseYear { get; set; }

    [Parameter]
    public int CurrentYear { get; set; }

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public bool ReferenceDataLoaded { get; set; }

    [Parameter]
    public EventCallback<GamesFiltersSubmission> OnSearchSubmitted { get; set; }

    [Parameter]
    public EventCallback OnClearAll { get; set; }

    [Parameter]
    public EventCallback OnRemoveSearch { get; set; }

    [Parameter]
    public EventCallback OnRemoveGenre { get; set; }

    [Parameter]
    public EventCallback OnRemovePlatform { get; set; }

    [Parameter]
    public EventCallback OnRemoveReleaseYear { get; set; }

    private string _searchInput = string.Empty;

    private Guid? _selectedGenreInput;

    private Guid? _selectedPlatformInput;

    private int? _releaseYearInput;

    private string? _lastAppliedSearchTerm;

    private Guid? _lastAppliedGenreId;

    private Guid? _lastAppliedPlatformId;

    private int? _lastAppliedReleaseYear;

    private bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(SearchTerm) ||
        SelectedGenreId.HasValue ||
        SelectedPlatformId.HasValue ||
        ReleaseYear.HasValue;

    private string SelectedGenreName =>
        Genres
            .FirstOrDefault(
                genre => genre.Id == SelectedGenreId)
            ?.Name ??
        "Genre";

    private string SelectedPlatformName =>
        Platforms
            .FirstOrDefault(
                platform => platform.Id == SelectedPlatformId)
            ?.Name ??
        "Platform";

    protected override void OnParametersSet()
    {
        if (!string.Equals(
                SearchTerm,
                _lastAppliedSearchTerm,
                StringComparison.Ordinal))
        {
            _searchInput =
                SearchTerm ?? string.Empty;

            _lastAppliedSearchTerm =
                SearchTerm;
        }

        if (SelectedGenreId != _lastAppliedGenreId)
        {
            _selectedGenreInput =
                SelectedGenreId;

            _lastAppliedGenreId =
                SelectedGenreId;
        }

        if (SelectedPlatformId != _lastAppliedPlatformId)
        {
            _selectedPlatformInput =
                SelectedPlatformId;

            _lastAppliedPlatformId =
                SelectedPlatformId;
        }

        if (ReleaseYear != _lastAppliedReleaseYear)
        {
            _releaseYearInput =
                ReleaseYear;

            _lastAppliedReleaseYear =
                ReleaseYear;
        }
    }

    private Task SubmitSearchAsync()
    {
        if (IsLoading)
        {
            return Task.CompletedTask;
        }

        var normalizedSearchTerm = string.IsNullOrWhiteSpace(_searchInput) ? null : _searchInput.Trim();

        var submission =
            new GamesFiltersSubmission(normalizedSearchTerm, _selectedGenreInput, _selectedPlatformInput, _releaseYearInput);

        return OnSearchSubmitted.InvokeAsync(
            submission);
    }
}