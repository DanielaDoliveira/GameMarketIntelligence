using Microsoft.AspNetCore.Components.Web;

namespace GameMarketIntel.Web.Layout;

public partial class MainLayout
{
    private bool IsDesktopSidebarCollapsed { get; set; }

    private bool IsMobileDrawerOpen { get; set; }

    private string ShellCssClass =>
        string.Join(
            " ",
            "application-shell",
            IsDesktopSidebarCollapsed
                ? "application-shell--sidebar-collapsed"
                : null,
            IsMobileDrawerOpen
                ? "application-shell--drawer-open"
                : null);

    private string MobileMenuButtonLabel =>
        IsMobileDrawerOpen
            ? "Close navigation"
            : "Open navigation";

    private string DesktopMenuButtonLabel =>
        IsDesktopSidebarCollapsed
            ? "Expand navigation"
            : "Collapse navigation";

    private void ToggleMobileDrawer()
    {
        IsMobileDrawerOpen =
            !IsMobileDrawerOpen;
    }

    private void ToggleDesktopSidebar()
    {
        IsDesktopSidebarCollapsed =
            !IsDesktopSidebarCollapsed;
    }

    private void CloseMobileDrawer()
    {
        IsMobileDrawerOpen = false;
    }

    private void HandleKeyDown(
        KeyboardEventArgs args)
    {
        if (args.Key == "Escape")
        {
            CloseMobileDrawer();
        }
    }
}