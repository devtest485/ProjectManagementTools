using ProjectManagementTools.Core.DTOs;
using ProjectManagementTools.Core.Entities.Auth;
using ProjectManagementTools.Core.ResponseObject;

namespace ProjectManagementTools.Core.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(LoginViewModel model);
        Task<AuthResult> RegisterAsync(RegisterViewModel model);
        Task<AuthResult> ForgotPasswordAsync(ForgotPasswordViewModel model);
        Task<AuthResult> ResetPasswordAsync(ResetPasswordViewModel model);
        Task<AuthResult> UpdateProfileAsync(UserProfileViewModel model);
        Task<AuthResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<ApplicationUser?> GetCurrentUserAsync();
        Task<UserProfileViewModel?> GetUserProfileAsync(string userId);
        Task LogoutAsync();
        Task<bool> IsEmailConfirmedAsync(string userId);
        Task<AuthResult> ResendEmailConfirmationAsync(string email);
    }
}
