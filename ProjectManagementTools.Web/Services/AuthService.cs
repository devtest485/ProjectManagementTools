using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagementTools.Core.DTOs;
using ProjectManagementTools.Core.Entities.Auth;
using ProjectManagementTools.Core.Interfaces.Services;
using ProjectManagementTools.Core.ResponseObject;

namespace ProjectManagementTools.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            IMapper mapper,
            ILogger<AuthService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<AuthResult> LoginAsync(LoginViewModel model)
        {
            // Validate model first
            if (model == null)
            {
                _logger.LogWarning("LoginAsync called with null model");
                return new AuthResult
                {
                    Success = false,
                    Message = "Invalid login request"
                };
            }

            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            {
                _logger.LogWarning("LoginAsync called with empty email or password");
                return new AuthResult
                {
                    Success = false,
                    Message = "Email and password are required"
                };
            }

            try
            {
                // Validate that we have a valid HTTP context
                if (_httpContextAccessor.HttpContext == null)
                {
                    _logger.LogError("HTTP context is not available for login operation");
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Service temporarily unavailable. Please try again."
                    };
                }

                ApplicationUser user = null;

                // Retry logic for database operations
                var maxRetries = 3;
                var retryCount = 0;

                while (retryCount < maxRetries)
                {
                    try
                    {
                        // Find user with retry logic
                        user = await _userManager.FindByEmailAsync(model.Email);
                        break; // Success, exit retry loop
                    }
                    catch (InvalidOperationException ex) when (ex.Message.Contains("connection is closed"))
                    {
                        retryCount++;
                        _logger.LogWarning(ex, "Database connection closed, retry attempt {RetryCount} for user {Email}",
                            retryCount, model.Email);

                        if (retryCount >= maxRetries)
                        {
                            _logger.LogError(ex, "Max retries exceeded for finding user {Email}", model.Email);
                            return new AuthResult
                            {
                                Success = false,
                                Message = "Service temporarily unavailable. Please try again."
                            };
                        }

                        // Wait before retry
                        await Task.Delay(TimeSpan.FromMilliseconds(500 * retryCount));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error finding user {Email}", model.Email);
                        throw; // Re-throw non-connection related exceptions
                    }
                }

                if (user == null)
                {
                    _logger.LogWarning("Login attempt for non-existent user {Email}", model.Email);
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Login attempt for inactive user {Email}", model.Email);
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Account is deactivated. Please contact administrator."
                    };
                }

                // Attempt sign in with retry logic
                retryCount = 0;
                Microsoft.AspNetCore.Identity.SignInResult result = null;

                while (retryCount < maxRetries)
                {
                    try
                    {
                        result = await _signInManager.PasswordSignInAsync(
                            user, model.Password, model.RememberMe, lockoutOnFailure: true);
                        break; // Success, exit retry loop
                    }
                    catch (InvalidOperationException ex) when (ex.Message.Contains("connection is closed"))
                    {
                        retryCount++;
                        _logger.LogWarning(ex, "Database connection closed during sign in, retry attempt {RetryCount} for user {Email}",
                            retryCount, model.Email);

                        if (retryCount >= maxRetries)
                        {
                            _logger.LogError(ex, "Max retries exceeded for sign in {Email}", model.Email);
                            return new AuthResult
                            {
                                Success = false,
                                Message = "Service temporarily unavailable. Please try again."
                            };
                        }

                        await Task.Delay(TimeSpan.FromMilliseconds(500 * retryCount));
                    }
                }

                if (result == null)
                {
                    _logger.LogError("Sign in result is null for user {Email}", model.Email);
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Service temporarily unavailable. Please try again."
                    };
                }

                if (result.Succeeded)
                {
                    // Update last login date with retry logic
                    retryCount = 0;
                    while (retryCount < maxRetries)
                    {
                        try
                        {
                            user.LastLoginDate = DateTime.UtcNow;
                            await _userManager.UpdateAsync(user);
                            break;
                        }
                        catch (InvalidOperationException ex) when (ex.Message.Contains("connection is closed"))
                        {
                            retryCount++;
                            _logger.LogWarning(ex, "Database connection closed during user update, retry attempt {RetryCount} for user {Email}",
                                retryCount, model.Email);

                            if (retryCount >= maxRetries)
                            {
                                // Log but don't fail the login for this
                                _logger.LogError(ex, "Failed to update last login date for user {Email}", model.Email);
                                break;
                            }

                            await Task.Delay(TimeSpan.FromMilliseconds(500 * retryCount));
                        }
                    }

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
                    _logger.LogWarning("User {Email} account is locked out", model.Email);
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Account locked due to multiple failed attempts. Try again later."
                    };
                }

                if (result.RequiresTwoFactor)
                {
                    _logger.LogInformation("User {Email} requires two-factor authentication", model.Email);
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Two-factor authentication required"
                    };
                }

                _logger.LogWarning("Failed login attempt for user {Email}", model.Email);
                return new AuthResult
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error during login for {Email}", model.Email);
                return new AuthResult
                {
                    Success = false,
                    Message = "Service temporarily unavailable. Please try again."
                };
            }
            catch (ObjectDisposedException ex)
            {
                _logger.LogError(ex, "Service has been disposed during login for {Email}", model.Email);
                return new AuthResult
                {
                    Success = false,
                    Message = "Service temporarily unavailable. Please try again."
                };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation during login for {Email}. Context may be invalid.", model.Email);
                return new AuthResult
                {
                    Success = false,
                    Message = "Service temporarily unavailable. Please try again."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for {Email}", model.Email);
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
            catch (ObjectDisposedException ex)
            {
                _logger.LogError(ex, "UserManager has been disposed during registration for {Email}", model.Email);
                return new AuthResult
                {
                    Success = false,
                    Message = "Service temporarily unavailable. Please try again."
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
            catch (ObjectDisposedException ex)
            {
                _logger.LogError(ex, "UserManager has been disposed during password reset for {Email}", model.Email);
                return new AuthResult
                {
                    Success = false,
                    Message = "Service temporarily unavailable. Please try again."
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
            catch (ObjectDisposedException ex)
            {
                _logger.LogError(ex, "UserManager has been disposed during password reset for {Email}", model.Email);
                return new AuthResult
                {
                    Success = false,
                    Message = "Service temporarily unavailable. Please try again."
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
            try
            {
                if (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true)
                {
                    return await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                }
                return null;
            }
            catch (ObjectDisposedException ex)
            {
                _logger.LogError(ex, "UserManager has been disposed while getting current user");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return null;
            }
        }

        public async Task<UserProfileViewModel?> GetUserProfileAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                return user != null ? _mapper.Map<UserProfileViewModel>(user) : null;
            }
            catch (ObjectDisposedException ex)
            {
                _logger.LogError(ex, "UserManager has been disposed while getting user profile for {UserId}", userId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile for {UserId}", userId);
                return null;
            }
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
            catch (ObjectDisposedException ex)
            {
                _logger.LogError(ex, "UserManager has been disposed while updating profile for user {UserId}", model.Id);
                return new AuthResult
                {
                    Success = false,
                    Message = "Service temporarily unavailable. Please try again."
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
            catch (ObjectDisposedException ex)
            {
                _logger.LogError(ex, "UserManager has been disposed while changing password for user {UserId}", userId);
                return new AuthResult
                {
                    Success = false,
                    Message = "Service temporarily unavailable. Please try again."
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
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    await _signInManager.SignOutAsync();
                }
            }
            catch (ObjectDisposedException ex)
            {
                _logger.LogError(ex, "SignInManager has been disposed during logout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
            }
        }

        public async Task<bool> IsEmailConfirmedAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                return user?.EmailConfirmed ?? false;
            }
            catch (ObjectDisposedException ex)
            {
                _logger.LogError(ex, "UserManager has been disposed while checking email confirmation for user {UserId}", userId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email confirmation for user {UserId}", userId);
                return false;
            }
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
            catch (ObjectDisposedException ex)
            {
                _logger.LogError(ex, "UserManager has been disposed while resending email confirmation for {Email}", email);
                return new AuthResult
                {
                    Success = false,
                    Message = "Service temporarily unavailable. Please try again."
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
