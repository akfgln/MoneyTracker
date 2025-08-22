using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Application.DTOs.Auth;
using MoneyTracker.Domain.Entities;
using System.Security.Claims;

namespace MoneyTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IGdprService _gdprService;
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IJwtService jwtService,
        IEmailService emailService,
        IGdprService gdprService,
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _jwtService = jwtService;
        _emailService = emailService;
        _gdprService = gdprService;
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user account with German data protection compliance
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        try
        {
            // Check if user already exists
            if (await _userManager.FindByEmailAsync(request.Email) != null)
            {
                return BadRequest(new { message = "Ein Benutzer mit dieser E-Mail-Adresse existiert bereits." });
            }

            // Create user entity
            var user = _mapper.Map<User>(request);
            user.UserName = request.Email;
            user.EmailConfirmationToken = Guid.NewGuid().ToString();
            user.EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24);

            // Create user with password
            var result = await _userManager.CreateAsync(user, request.Password);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = "Registrierung fehlgeschlagen.", errors });
            }

            // Assign default role
            await _userManager.AddToRoleAsync(user, "User");

            // Send confirmation email
            await _emailService.SendEmailConfirmationAsync(user.Email!, user.FirstName, user.EmailConfirmationToken);

            // Create user consent record
            var userConsent = new UserConsent
            {
                UserId = user.Id,
                PrivacyPolicyAccepted = request.AcceptPrivacyPolicy,
                PrivacyPolicyAcceptedDate = DateTime.UtcNow,
                DataProcessingConsent = true,
                PrivacyPolicyVersion = "1.0",
                TermsOfServiceAccepted = request.AcceptTermsOfService,
                TermsOfServiceAcceptedDate = DateTime.UtcNow,
                ConsentIpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                ConsentUserAgent = Request.Headers.UserAgent.ToString()
            };

            // Save consent to database
            _context.UserConsents.Add(userConsent);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User registered successfully: {Email}", request.Email);
            
            return Ok(new 
            { 
                message = "Registrierung erfolgreich. Bitte überprüfen Sie Ihre E-Mail zur Bestätigung.",
                userId = user.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration: {Email}", request.Email);
            return StatusCode(500, new { message = "Ein interner Serverfehler ist aufgetreten." });
        }
    }

    /// <summary>
    /// User login with credential validation
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        try
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest(new { message = "Ungültige Anmeldedaten." });
            }

            // Check if user is active
            if (!user.IsActive)
            {
                return BadRequest(new { message = "Ihr Konto wurde deaktiviert. Bitte kontaktieren Sie den Support." });
            }

            // Verify password
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            
            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    return BadRequest(new { message = "Konto gesperrt aufgrund mehrerer fehlgeschlagener Anmeldeversuche." });
                }
                
                return BadRequest(new { message = "Ungültige Anmeldedaten." });
            }

            // Check if email is confirmed (optional based on requirements)
            if (!user.EmailConfirmed && false) // Set to true to enforce email confirmation
            {
                return BadRequest(new { message = "Bitte bestätigen Sie Ihre E-Mail-Adresse." });
            }

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Save refresh token
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty
            };

            // Save refresh token to database
            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            // Update last login
            user.UpdateLastLogin();
            await _userManager.UpdateAsync(user);

            // Create response
            var userProfile = _mapper.Map<UserProfileDto>(user);
            userProfile.Roles = roles.ToList();

            var response = new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(24), // Based on JWT settings
                User = userProfile
            };

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login: {Email}", request.Email);
            return StatusCode(500, new { message = "Ein interner Serverfehler ist aufgetreten." });
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto request)
    {
        try
        {
            // Validate the current access token (even if expired)
            var principal = _jwtService.GetPrincipalFromExpiredToken(request.RefreshToken);
            if (principal == null)
            {
                return BadRequest(new { message = "Ungültiger Token." });
            }

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userIdGuid))
            {
                return BadRequest(new { message = "Ungültiger Token." });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                return BadRequest(new { message = "Benutzer nicht gefunden oder deaktiviert." });
            }

            // Validate refresh token (check against database)
            var storedRefreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.UserId == userIdGuid && rt.IsActive);
                
            if (storedRefreshToken == null || storedRefreshToken.IsExpired || storedRefreshToken.IsRevoked)
            {
                return BadRequest(new { message = "Ungültiger oder abgelaufener Refresh Token." });
            }
            
            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Generate new tokens
            var newAccessToken = _jwtService.GenerateAccessToken(user, roles);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // Revoke old refresh token and create new one (token rotation)
            storedRefreshToken.RevokedAt = DateTime.UtcNow;
            storedRefreshToken.RevokedByIp = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
            storedRefreshToken.ReplacedByToken = newRefreshToken;
            storedRefreshToken.ReasonRevoked = "Replaced by new token";
            
            // Create new refresh token
            var newRefreshTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty
            };
            
            _context.RefreshTokens.Add(newRefreshTokenEntity);
            await _context.SaveChangesAsync();

            var userProfile = _mapper.Map<UserProfileDto>(user);
            userProfile.Roles = roles.ToList();

            var response = new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                User = userProfile
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return BadRequest(new { message = "Token-Erneuerung fehlgeschlagen." });
        }
    }

    /// <summary>
    /// Logout user and revoke refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "Ungültiger Benutzer." });
            }

            // Revoke all refresh tokens for the user
            var activeRefreshTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == Guid.Parse(userId) && rt.IsActive)
                .ToListAsync();
                
            foreach (var token in activeRefreshTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedByIp = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
                token.ReasonRevoked = "User logout";
            }
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("User logged out successfully: {UserId}", userId);
            
            return Ok(new { message = "Erfolgreich abgemeldet." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { message = "Ein interner Serverfehler ist aufgetreten." });
        }
    }

    /// <summary>
    /// Confirm email address using confirmation token
    /// </summary>
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return BadRequest(new { message = "Benutzer nicht gefunden." });
            }

            // Validate token
            if (user.EmailConfirmationToken != request.Token || 
                !user.EmailConfirmationTokenExpiry.HasValue ||
                user.EmailConfirmationTokenExpiry.Value < DateTime.UtcNow)
            {
                return BadRequest(new { message = "Ungültiger oder abgelaufener Bestätigungstoken." });
            }

            // Confirm email
            user.ConfirmEmail();
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Email confirmed successfully: {UserId}", request.UserId);
            
            return Ok(new { message = "E-Mail erfolgreich bestätigt." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming email: {UserId}", request.UserId);
            return StatusCode(500, new { message = "Ein interner Serverfehler ist aufgetreten." });
        }
    }

    /// <summary>
    /// Request password reset token
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Don't reveal if user exists - return success anyway for security
                return Ok(new { message = "Falls ein Konto mit dieser E-Mail existiert, wurde eine Zurücksetzungs-E-Mail gesendet." });
            }

            // Generate password reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            
            await _userManager.UpdateAsync(user);

            // Send password reset email
            await _emailService.SendPasswordResetAsync(user.Email!, user.FirstName, resetToken);

            _logger.LogInformation("Password reset requested: {Email}", request.Email);
            
            return Ok(new { message = "Falls ein Konto mit dieser E-Mail existiert, wurde eine Zurücksetzungs-E-Mail gesendet." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting password reset: {Email}", request.Email);
            return StatusCode(500, new { message = "Ein interner Serverfehler ist aufgetreten." });
        }
    }

    /// <summary>
    /// Reset password using reset token
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest(new { message = "Ungültige Anfrage." });
            }

            // Validate reset token
            if (user.PasswordResetToken != request.Token ||
                !user.PasswordResetTokenExpiry.HasValue ||
                user.PasswordResetTokenExpiry.Value < DateTime.UtcNow)
            {
                return BadRequest(new { message = "Ungültiger oder abgelaufener Zurücksetzungstoken." });
            }

            // Reset password
            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = "Passwort-Zurücksetzung fehlgeschlagen.", errors });
            }

            // Clear reset token
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Password reset successfully: {Email}", request.Email);
            
            return Ok(new { message = "Passwort erfolgreich zurückgesetzt." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password: {Email}", request.Email);
            return StatusCode(500, new { message = "Ein interner Serverfehler ist aufgetreten." });
        }
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "Ungültiger Benutzer." });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "Benutzer nicht gefunden." });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userProfile = _mapper.Map<UserProfileDto>(user);
            userProfile.Roles = roles.ToList();

            return Ok(userProfile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile");
            return StatusCode(500, new { message = "Ein interner Serverfehler ist aufgetreten." });
        }
    }

    /// <summary>
    /// Update user profile information
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "Ungültiger Benutzer." });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "Benutzer nicht gefunden." });
            }

            // Update user properties
            _mapper.Map(request, user);
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = "Profil-Update fehlgeschlagen.", errors });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userProfile = _mapper.Map<UserProfileDto>(user);
            userProfile.Roles = roles.ToList();

            _logger.LogInformation("User profile updated: {UserId}", userId);
            
            return Ok(userProfile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, new { message = "Ein interner Serverfehler ist aufgetreten." });
        }
    }

    /// <summary>
    /// Delete user account (GDPR compliance - Right to be Forgotten)
    /// </summary>
    [HttpDelete("account")]
    [Authorize]
    public async Task<IActionResult> DeleteAccount()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userIdGuid))
            {
                return BadRequest(new { message = "Ungültiger Benutzer." });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "Benutzer nicht gefunden." });
            }

            // Use GDPR service to handle data deletion
            var deleted = await _gdprService.DeleteUserDataAsync(userIdGuid);
            
            if (!deleted)
            {
                return StatusCode(500, new { message = "Fehler beim Löschen der Benutzerdaten." });
            }

            _logger.LogInformation("User account deleted (GDPR): {UserId}", userId);
            
            return Ok(new { message = "Ihr Konto und alle damit verbundenen Daten wurden erfolgreich gelöscht." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user account");
            return StatusCode(500, new { message = "Ein interner Serverfehler ist aufgetreten." });
        }
    }

    /// <summary>
    /// Export user data (GDPR compliance - Data Portability)
    /// </summary>
    [HttpGet("export-data")]
    [Authorize]
    public async Task<IActionResult> ExportUserData()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userIdGuid))
            {
                return BadRequest(new { message = "Ungültiger Benutzer." });
            }

            var exportData = await _gdprService.ExportUserDataAsync(userIdGuid);
            
            if (exportData == null)
            {
                return NotFound(new { message = "Keine Benutzerdaten zum Exportieren gefunden." });
            }

            _logger.LogInformation("User data exported (GDPR): {UserId}", userId);
            
            return File(exportData, "application/json", $"UserData_{userIdGuid}_{DateTime.UtcNow:yyyyMMdd}.json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting user data");
            return StatusCode(500, new { message = "Ein interner Serverfehler ist aufgetreten." });
        }
    }
}