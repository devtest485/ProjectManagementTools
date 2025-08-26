using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace ProjectManagementTools.Web.Components.Layout.Menus
{
    public partial class UserMenuComponent
    {
        [CascadingParameter]
        private Task<AuthenticationState>? AuthenticationState { get; set; }

        private string GetUserName()
        {
            if (AuthenticationState != null)
            {
                var task = AuthenticationState;
                if (task.IsCompleted)
                {
                    var authState = task.Result;
                    var user = authState.User;
                    if (user?.Identity?.IsAuthenticated == true)
                    {
                        var firstName = user.FindFirst("FirstName")?.Value;
                        var lastName = user.FindFirst("LastName")?.Value;
                        if (!string.IsNullOrEmpty(firstName))
                        {
                            return $"{firstName} {lastName}".Trim();
                        }
                        return user.Identity.Name ?? "User";
                    }
                }
            }
            return "User";
        }

        private async Task HandleLogout()
        {
            try
            {
                // Implement logout logic here
                Navigation.NavigateTo("/account/logout", forceLoad: true);
                Snackbar.Add("You have been signed out successfully", Severity.Info);
            }
            catch (Exception ex)
            {
                Snackbar.Add("Error signing out. Please try again.", Severity.Error);
            }
        }
    }
}
