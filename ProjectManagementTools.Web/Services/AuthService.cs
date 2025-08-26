using AutoMapper;
using Microsoft.AspNetCore.Identity;
using ProjectManagementTools.Core.DTOs;
using ProjectManagementTools.Core.Entities.Auth;
using ProjectManagementTools.Core.Interfaces.Services;
using ProjectManagementTools.Core.ResponseObject;

namespace ProjectManagementTools.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            IMapper mapper,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AuthResult> LoginAsync(LoginViewModel model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                if (!user.IsActive)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Account is deactivated. Please contact administrator."
                    };
                }

                var result = await _signInManager.PasswordSignInAsync(
                    user, model.Password, model.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    user.LastLoginDate = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    _logger.LogInformation("User {Email} logged in successfully", model.Email);
                    return new AuthResult
                    {
                        Success = true,
                        Message = "Login successful",
                        Data = user
                    };
                }

                if (result.IsLockedOut)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Account locked due to multiple failed attempts. Try again later."
                    };
                }

                if (result.RequiresTwoFactor)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Two-factor authentication required"
                    };
                }

                return new AuthResult
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", model.Email);
                return new AuthResult
                {
                    Success = false,
                    Message = "An error occurred during login"
                };
            }
        }

        public async Task<AuthResult> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Email already registered"
                    };
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Position = model.Position,
                    Department = model.Department,
                    Company = model.Company,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Add default role
                    await _userManager.AddToRoleAsync(user, "Member");

                    // Send email confirmation
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _emailService.SendEmailConfirmationAsync(user.Email, user.FullName, token);

                    _logger.LogInformation("User {Email} registered successfully", model.Email);
                    return new AuthResult
                    {
                        Success = true,
                        Message = "Registration successful. Please check your email to confirm your account.",
                        Data = user
                    };
                }

                return new AuthResult
                {
                    Success = false,
                    Message = "Registration failed",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for {Email}", model.Email);
                return new AuthResult
                {
                    Success = false,
                    Message = "An error occurred during registration"
                };
            }
        }

        public async Task<AuthResult> ForgotPasswordAsync(ForgotPasswordViewModel model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that user doesn't exist
                    return new AuthResult
                    {
                        Success = true,
                        Message = "If an account with that email exists, a password reset link has been sent."
                    };
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _emailService.SendPasswordResetAsync(user.Email, user.FullName, token);

                _logger.LogInformation("Password reset requested for {Email}", model.Email);
                return new AuthResult
                {
                    Success = true,
                    Message = "If an account with that email exists, a password reset link has been sent."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset for {Email}", model.Email);
                return new AuthResult
                {
                    Success = false,
                    Message = "An error occurred while processing your request"
                };
            }
        }

        public async Task<AuthResult> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Invalid reset token"
                    };
                }

                var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Password reset successful for {Email}", model.Email);
                    return new AuthResult
                    {
                        Success = true,
                        Message = "Password reset successful. You can now login with your new password."
                    };
                }

                return new AuthResult
                {
                    Success = false,
                    Message = "Password reset failed",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset for {Email}", model.Email);
                return new AuthResult
                {
                    Success = false,
                    Message = "An error occurred while resetting password"
                };
            }
        }

        public async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(_signInManager.Context.User);
        }

        public async Task<UserProfileViewModel?> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user != null ? _mapper.Map<UserProfileViewModel>(user) : null;
        }

        public async Task<AuthResult> UpdateProfileAsync(UserProfileViewModel model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.Id.ToString());
                if (user == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.UserName = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.Position = model.Position;
                user.Department = model.Department;
                user.Company = model.Company;
                user.Bio = model.Bio;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return new AuthResult
                    {
                        Success = true,
                        Message = "Profile updated successfully"
                    };
                }

                return new AuthResult
                {
                    Success = false,
                    Message = "Failed to update profile",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", model.Id);
                return new AuthResult
                {
                    Success = false,
                    Message = "An error occurred while updating profile"
                };
            }
        }

        public async Task<AuthResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

                if (result.Succeeded)
                {
                    return new AuthResult
                    {
                        Success = true,
                        Message = "Password changed successfully"
                    };
                }

                return new AuthResult
                {
                    Success = false,
                    Message = "Failed to change password",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return new AuthResult
                {
                    Success = false,
                    Message = "An error occurred while changing password"
                };
            }
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<bool> IsEmailConfirmedAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.EmailConfirmed ?? false;
        }

        public async Task<AuthResult> ResendEmailConfirmationAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return new AuthResult
                    {
                        Success = true,
                        Message = "If an account with that email exists, a confirmation email has been sent."
                    };
                }

                if (user.EmailConfirmed)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Email is already confirmed."
                    };
                }

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _emailService.SendEmailConfirmationAsync(user.Email, user.FullName, token);

                return new AuthResult
                {
                    Success = true,
                    Message = "Confirmation email sent successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending email confirmation for {Email}", email);
                return new AuthResult
                {
                    Success = false,
                    Message = "An error occurred while sending confirmation email"
                };
            }
        }
    }

}
