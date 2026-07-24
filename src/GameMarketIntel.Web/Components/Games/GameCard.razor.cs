
using GameMarketIntel.Shared.Contracts.Games.Search;
using Microsoft.AspNetCore.Components;

namespace GameMarketIntel.Web.Components.Games;

public partial class GameCard
{
    private const int VisibleCategoryCount = 2;

    [Parameter, EditorRequired] public GameSearchItem Game { get; set; } = default!;

    private bool _imageLoadFailed;

    private bool ShouldShowImage => !_imageLoadFailed && !string.IsNullOrWhiteSpace(Game.ImageUrl);

    private string DetailsUrl => $"/games/{Game.Id}";

    private void HandleImageError()
    {
        _imageLoadFailed = true;
    }

    private static string FormatCategories(IReadOnlyCollection<GameSearchCategory> categories)
    {
        var visibleCategories =
            categories
                .Take(VisibleCategoryCount)
                .Select(category => category.Name)
                .ToList();

        var hiddenCategoryCount =
            categories.Count - visibleCategories.Count;

        var formattedCategories =
            string.Join(", ", visibleCategories);

        return hiddenCategoryCount > 0
            ? $"{formattedCategories} +{hiddenCategoryCount} more"
            : formattedCategories;
    }
}