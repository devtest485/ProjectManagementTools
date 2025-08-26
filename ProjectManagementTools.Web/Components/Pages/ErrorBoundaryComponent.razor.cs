using Microsoft.AspNetCore.Components;

namespace ProjectManagementTools.Web.Components.Pages
{
    public partial class ErrorBoundaryComponent : ErrorBoundaryBase
    {
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public bool ShowDetails { get; set; } = false;
        [Parameter] public bool ShowStackTrace { get; set; } = false;

        protected override Task OnErrorAsync(Exception exception)
        {
            Logger.LogError(exception, "Error caught by error boundary");
            return Task.CompletedTask; // Provide a concrete implementation
        }
    }
}
