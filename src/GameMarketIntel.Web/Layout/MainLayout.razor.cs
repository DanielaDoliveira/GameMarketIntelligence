using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;

namespace GameMarketIntel.Web.Layout;

public partial class MainLayout : IDisposable
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private bool IsDesktopSidebarCollapsed { get; set; }

    private bool IsMobileDrawerOpen { get; set; }

    private string _searchText = string.Empty;

    private string ShellCssClass =>
        string.Join(" ", "application-shell", IsDesktopSidebarCollapsed ? "application-shell--sidebar-collapsed"
                : null, IsMobileDrawerOpen ? "application-shell--drawer-open" : null);

    private string MobileMenuButtonLabel =>
        IsMobileDrawerOpen
            ? "Close navigation"
            : "Open navigation";

    private string DesktopMenuButtonLabel =>
        IsDesktopSidebarCollapsed
            ? "Expand navigation"
            : "Collapse navigation";

    protected override void OnInitialized()
    {
        SynchronizeSearchWithUrl();

        NavigationManager.LocationChanged +=
            HandleLocationChanged;
    }

    private void SubmitSearch()
    {
        var currentUri =
            NavigationManager.ToAbsoluteUri(
                NavigationManager.Uri);

        var normalizedSearch =
            NormalizeSearch(_searchText);

        if (!IsComparableGamesPage(currentUri))
        {
            NavigateToComparableGames(normalizedSearch);

            return;
        }

        var query = System.Web.HttpUtility.ParseQueryString(currentUri.Query);

        query.Remove("page");

        if (normalizedSearch is null)
        {
            query.Remove("search");
        }
        else
        {
            query["search"] = normalizedSearch;
        }

        NavigateToComparableGamesQuery(query);
    }

    private void HandleLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        SynchronizeSearchWithUrl();

        _ = InvokeAsync(StateHasChanged);
    }

    private void SynchronizeSearchWithUrl()
    {
        var currentUri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        
        var query = System.Web.HttpUtility.ParseQueryString(currentUri.Query);

        _searchText = query["search"] ?? string.Empty;
    }

    private void NavigateToComparableGames(string? search)
    {
        if (search is null)
        {
            NavigationManager.NavigateTo("/games");
            return;
        }

        var encodedSearch = Uri.EscapeDataString(search);

        NavigationManager.NavigateTo($"/games?search={encodedSearch}");
    }

    private void NavigateToComparableGamesQuery(System.Collections.Specialized.NameValueCollection query)
    {
        var queryString = query.ToString();

        var targetUri = string.IsNullOrWhiteSpace(queryString) ? "/games" : $"/games?{queryString}";

        NavigationManager.NavigateTo(targetUri);
    }

    private static bool IsComparableGamesPage(Uri uri)
    {
        return uri.AbsolutePath.Equals("/games", StringComparison.OrdinalIgnoreCase);
    }

    private static string? NormalizeSearch(string? search)
    {
        return string.IsNullOrWhiteSpace(search) ? null : search.Trim();
    }

    private void ToggleMobileDrawer()
    {
        IsMobileDrawerOpen = !IsMobileDrawerOpen;
    }

    private void ToggleDesktopSidebar()
    {
        IsDesktopSidebarCollapsed = !IsDesktopSidebarCollapsed;
    }

    private void CloseMobileDrawer()
    {
        IsMobileDrawerOpen = false;
    }

    private void HandleKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "Escape")
        {
            CloseMobileDrawer();
        }
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= HandleLocationChanged;
    }
}