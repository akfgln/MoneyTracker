using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Application.DTOs.Auth;
using MoneyTracker.Domain.Constants;
using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    // SignInManager is commented out for compilation fix - can be re-added later
    // private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<User> userManager,
        RoleManager<ApplicationRole> roleManager,
        IJwtService jwtService,
        IEmailService emailService,
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtService = jwtService;
        _emailService = emailService;
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto request, string? ipAddress = null)
    {
        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Ein Benutzer mit dieser E-Mail-Adresse existiert bereits.");
        }

        // Create new user
        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PreferredLanguage = request.PreferredLanguage,
            AcceptedTerms = request.AcceptTermsOfService,
            AcceptedPrivacyPolicy = request.AcceptPrivacyPolicy,
            TermsAcceptedDate = DateTime.UtcNow,
            PrivacyPolicyAcceptedDate = DateTime.UtcNow,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            Country = request.Country,
            EmailConfirmed = false, // Will be confirmed via email
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Benutzer konnte nicht erstellt werden: {errors}");
        }

        // Add user to default role
        await _userManager.AddToRoleAsync(user, UserRoles.User);

        // Create user consent record
        var userConsent = new UserConsent
        {
            UserId = user.Id,
            PrivacyPolicyAccepted = request.AcceptPrivacyPolicy,
            PrivacyPolicyAcceptedDate = DateTime.UtcNow,
            TermsOfServiceAccepted = request.AcceptTermsOfService,
            TermsOfServiceAcceptedDate = DateTime.UtcNow,
            MarketingEmailsConsent = request.MarketingEmailsConsent,
            DataProcessingConsent = true,
            CookieConsent = true,
            ConsentIpAddress = ipAddress
        };

        _context.Set<UserConsent>().Add(userConsent);
        await _context.SaveChangesAsync(default);

        // Generate email confirmation token
        var emailToken = await _jwtService.GenerateEmailConfirmationTokenAsync(user);
        user.EmailConfirmationToken = emailToken;
        user.EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24);
        await _userManager.UpdateAsync(user);

        // Send confirmation email
        await _emailService.SendEmailConfirmationAsync(user.Email!, user.FirstName, emailToken);

        // Generate tokens for response
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress ?? "Unknown"
        };

        _context.Set<RefreshToken>().Add(refreshTokenEntity);
        await _context.SaveChangesAsync(default);

        _logger.LogInformation("User {Email} registered successfully", request.Email);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            User = _mapper.Map<UserProfileDto>(user)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto request, string? ipAddress = null)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Ungültige Anmeldedaten.");
        }

        // Use UserManager to check password instead of SignInManager
        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            throw new UnauthorizedAccessException("Ungültige Anmeldedaten.");
        }

        // Update last login date
        user.LastLoginDate = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Generate tokens
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Revoke old refresh tokens if not remember me
        if (!request.RememberMe)
        {
            var oldTokens = await _context.Set<RefreshToken>()
                .Where(rt => rt.UserId == user.Id && rt.IsActive)
                .ToListAsync();
            
            foreach (var token in oldTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedByIp = ipAddress;
                token.ReasonRevoked = "New login without remember me";
            }
        }

        // Save new refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(request.RememberMe ? 30 : 7),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress ?? "Unknown"
        };

        _context.Set<RefreshToken>().Add(refreshTokenEntity);
        await _context.SaveChangesAsync(default);

        _logger.LogInformation("User {Email} logged in successfully", request.Email);

        var userProfile = _mapper.Map<UserProfileDto>(user);
        userProfile.Roles = roles;

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = _jwtService.GetTokenExpiration(accessToken),
            User = userProfile
        };
    }

    // Additional methods will be implemented in the next part...
    public Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string? ipAddress = null)
    {
        throw new NotImplementedException("Will be implemented in next iteration");
    }

    public Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress = null)
    {
        throw new NotImplementedException("Will be implemented in next iteration");
    }

    public Task<bool> ConfirmEmailAsync(string userId, string token)
    {
        throw new NotImplementedException("Will be implemented in next iteration");
    }

    public Task<bool> ForgotPasswordAsync(string email)
    {
        throw new NotImplementedException("Will be implemented in next iteration");
    }

    public Task<bool> ResetPasswordAsync(ResetPasswordDto request)
    {
        throw new NotImplementedException("Will be implemented in next iteration");
    }

    public Task<UserProfileDto?> GetUserProfileAsync(Guid userId)
    {
        throw new NotImplementedException("Will be implemented in next iteration");
    }

    public Task<UserProfileDto?> UpdateUserProfileAsync(Guid userId, UpdateProfileDto request)
    {
        throw new NotImplementedException("Will be implemented in next iteration");
    }

    public Task<bool> DeleteUserAccountAsync(Guid userId)
    {
        throw new NotImplementedException("Will be implemented in next iteration");
    }

    public Task<UserConsentDto?> GetUserConsentAsync(Guid userId)
    {
        throw new NotImplementedException("Will be implemented in next iteration");
    }

    public Task<bool> UpdateUserConsentAsync(Guid userId, UserConsentDto consent, string? ipAddress = null)
    {
        throw new NotImplementedException("Will be implemented in next iteration");
    }
}