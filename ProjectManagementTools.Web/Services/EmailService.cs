using Microsoft.Extensions.Options;
using ProjectManagementTools.Core.DTOs;
using ProjectManagementTools.Core.Interfaces.Services;

namespace ProjectManagementTools.Web.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailOptions _emailOptions;
        private readonly ILogger<EmailService> _logger;
        private readonly LinkGenerator _linkGenerator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EmailService(
            IOptions<EmailOptions> emailOptions,
            ILogger<EmailService> logger,
            LinkGenerator linkGenerator,
            IHttpContextAccessor httpContextAccessor)
        {
            _emailOptions = emailOptions.Value;
            _logger = logger;
            _linkGenerator = linkGenerator;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task SendEmailConfirmationAsync(string email, string name, string token)
        {
            var confirmationLink = GenerateConfirmationLink(email, token);
            var subject = "Confirm Your Email - ProjectFlow";
            var body = $@"
                <h2>Welcome to ProjectFlow, {name}!</h2>
                <p>Please confirm your email address by clicking the link below:</p>
                <a href='{confirmationLink}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                    Confirm Email
                </a>
                <p>If you didn't create this account, please ignore this email.</p>
            ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetAsync(string email, string name, string token)
        {
            var resetLink = GeneratePasswordResetLink(email, token);
            var subject = "Reset Your Password - ProjectFlow";
            var body = $@"
                <h2>Password Reset Request</h2>
                <p>Hello {name},</p>
                <p>You requested a password reset. Click the link below to reset your password:</p>
                <a href='{resetLink}' style='background-color: #dc3545; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                    Reset Password
                </a>
                <p>This link will expire in 24 hours.</p>
                <p>If you didn't request this reset, please ignore this email.</p>
            ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendWelcomeEmailAsync(string email, string name)
        {
            var subject = "Welcome to ProjectFlow!";
            var body = $@"
                <h2>Welcome to ProjectFlow, {name}!</h2>
                <p>Your account has been created successfully. You can now start managing your projects!</p>
                <p>Get started by creating your first project or joining an existing team.</p>
            ";

            await SendEmailAsync(email, subject, body);
        }

        private async Task SendEmailAsync(string email, string subject, string body)
        {
            try
            {
                // Implement actual email sending logic here
                // For now, just log the email (replace with actual email service like SendGrid, SMTP, etc.)
                _logger.LogInformation("Email sent to {Email} with subject {Subject}", email, subject);

                // TODO: Implement actual email sending
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", email);
                throw;
            }
        }

        private string GenerateConfirmationLink(string email, string token)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return _linkGenerator.GetUriByPage(
                httpContext,
                "/Account/ConfirmEmail",
                values: new { email, token });
        }

        private string GeneratePasswordResetLink(string email, string token)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return _linkGenerator.GetUriByPage(
                httpContext,
                "/Account/ResetPassword",
                values: new { email, token });
        }
    }
}
