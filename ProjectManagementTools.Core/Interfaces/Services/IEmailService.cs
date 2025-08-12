namespace ProjectManagementTools.Core.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string email, string name, string token);
        Task SendPasswordResetAsync(string email, string name, string token);
        Task SendWelcomeEmailAsync(string email, string name);
    }
}
