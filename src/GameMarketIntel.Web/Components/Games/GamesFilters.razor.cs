using GameMarketIntel.Shared.Contracts.Genres;
using GameMarketIntel.Shared.Contracts.Platforms;
using Microsoft.AspNetCore.Components;

namespace GameMarketIntel.Web.Components.Games;

public partial class GamesFilters
{
    [Parameter] public IReadOnlyList<GenreDetails> Genres { get; set; } = [];

    [Parameter] public IReadOnlyList<PlatformDetails> Platforms { get; set; } = [];

    [Parameter] public string? SearchTerm { get; set; }

    [Parameter] public Guid? SelectedGenreId { get; set; }

    [Parameter] public Guid? SelectedPlatformId { get; set; }

    [Parameter] public int? ReleaseYear { get; set; }

    [Parameter] public int CurrentYear { get; set; }

    [Parameter] public bool IsLoading { get; set; }

    [Parameter] public bool ReferenceDataLoaded { get; set; }

    [Parameter] public EventCallback<ChangeEventArgs> OnGenreChanged { get; set; }

    [Parameter] public EventCallback<ChangeEventArgs> OnPlatformChanged { get; set; }

    [Parameter] public EventCallback<ChangeEventArgs> OnReleaseYearChanged { get; set; }

    [Parameter] public EventCallback OnClearAll { get; set; }

    [Parameter] public EventCallback OnRemoveSearch { get; set; }

    [Parameter] public EventCallback OnRemoveGenre { get; set; }

    [Parameter] public EventCallback OnRemovePlatform { get; set; }

    [Parameter] public EventCallback OnRemoveReleaseYear { get; set; }
    
    private bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(SearchTerm) ||
        SelectedGenreId.HasValue ||
        SelectedPlatformId.HasValue ||
        ReleaseYear.HasValue;
    
    
    private string SelectedGenreValue => SelectedGenreId?.ToString() ?? string.Empty;

    private string SelectedPlatformValue => SelectedPlatformId?.ToString() ?? string.Empty;
    
    
    private string ReleaseYearValue => ReleaseYear?.ToString() ?? string.Empty;
    
    
    
    private string SelectedGenreName => 
        Genres.FirstOrDefault(genre => genre.Id == SelectedGenreId)?.Name ?? "Genre";
    
    private string SelectedPlatformName => 
        Platforms.FirstOrDefault(platform => platform.Id == SelectedPlatformId)?.Name ?? "Platform";
    
    
    
    
}