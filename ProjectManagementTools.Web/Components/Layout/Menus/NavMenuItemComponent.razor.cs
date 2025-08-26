using Microsoft.AspNetCore.Components;

namespace ProjectManagementTools.Web.Components.Layout.Menus
{
    public partial class NavMenuItemComponent
    {
        [Parameter] public string Href { get; set; } = "";
        [Parameter] public string Icon { get; set; } = "";
        [Parameter] public string Text { get; set; } = "";
        [Parameter] public int Badge { get; set; } = 0;
        [Parameter] public bool Exact { get; set; } = false;

        private string GetNavLinkClass()
        {
            var currentPath = Navigation.ToBaseRelativePath(Navigation.Uri);
            var isActive = Exact ? currentPath.Equals(Href.TrimStart('/'), StringComparison.OrdinalIgnoreCase)
                : currentPath.StartsWith(Href.TrimStart('/'), StringComparison.OrdinalIgnoreCase);
            return isActive ? "nav-link-active" : "";
        }
    }
}
