using System.Web;

namespace ProjectManagementTools.Web.Components.Pages
{
    public partial class RedirectToLogin
    {
        protected override void OnInitialized()
        {
            var returnUrl = HttpUtility.UrlEncode(Navigation.ToBaseRelativePath(Navigation.Uri));
            Navigation.NavigateTo($"/Account/Login?returnUrl={returnUrl}", replace: true);
        }
    }
}
