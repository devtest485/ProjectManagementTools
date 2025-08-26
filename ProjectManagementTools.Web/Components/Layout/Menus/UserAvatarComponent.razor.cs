using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using System.Security.Claims;

namespace ProjectManagementTools.Web.Components.Layout.Menus
{
    public partial class UserAvatarComponent
    {
        [Parameter]
        public Size Size { get; set; } = Size.Medium;
        [Parameter] public Color Color { get; set; } = Color.Primary;
        [Parameter] public bool ShowName { get; set; } = false;
        [Parameter] public bool ShowEmail { get; set; } = false;
        [Parameter] public string CssClass { get; set; } = "flex items-center";
        [Parameter] public string AvatarClass { get; set; } = "";
        [CascadingParameter] private Task<AuthenticationState>? AuthenticationState { get; set; }

        private string FirstName = "";
        private string LastName = "";
        private string FullName = "";
        private string Email = "";
        private string AvatarUrl = "";
        private string Initials = "";

        protected override async Task OnInitializedAsync()
        {
            if (AuthenticationState != null)
            {
                var authState = await AuthenticationState;
                var user = authState.User;
                if (user.Identity?.IsAuthenticated == true)
                {
                    FirstName = user.FindFirst("FirstName")?.Value ?? "";
                    LastName = user.FindFirst("LastName")?.Value ?? "";
                    FullName = user.FindFirst("FullName")?.Value ?? $" {FirstName}  {LastName} ";
                    Email = user.FindFirst(ClaimTypes.Email)?.Value ?? "";
                    AvatarUrl = user.FindFirst("AvatarUrl")?.Value ?? "";
                    Initials = $" {FirstName.FirstOrDefault()} {LastName.FirstOrDefault()} ".ToUpper();
                }
            }
        }
    }
}
