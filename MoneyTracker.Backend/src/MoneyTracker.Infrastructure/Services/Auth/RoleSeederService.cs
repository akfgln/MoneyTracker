using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Infrastructure.Services.Auth;

public class RoleSeederService
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<RoleSeederService> _logger;

    public RoleSeederService(
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<User> userManager,
        ILogger<RoleSeederService> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SeedRolesAsync()
    {
        try
        {
            var roles = new[] { "User", "Premium", "Admin" };

            foreach (var roleName in roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var role = new IdentityRole<Guid>(roleName)
                    {
                        Id = Guid.NewGuid(),
                        NormalizedName = roleName.ToUpper()
                    };

                    var result = await _roleManager.CreateAsync(role);
                    
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Role created successfully: {RoleName}", roleName);
                    }
                    else
                    {
                        _logger.LogError("Failed to create role {RoleName}: {Errors}", 
                            roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    _logger.LogInformation("Role already exists: {RoleName}", roleName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding roles");
        }
    }

    public async Task SeedDefaultAdminAsync()
    {
        try
        {
            const string adminEmail = "admin@moneytracker.de";
            const string adminPassword = "Admin123!@#$%^";

            var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin != null)
            {
                _logger.LogInformation("Default admin user already exists: {Email}", adminEmail);
                return;
            }

            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
                PreferredLanguage = "de-DE",
                PreferredCurrency = "EUR",
                Country = "Germany",
                AcceptedTerms = true,
                AcceptedPrivacyPolicy = true,
                TermsAcceptedDate = DateTime.UtcNow,
                PrivacyPolicyAcceptedDate = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };

            var result = await _userManager.CreateAsync(adminUser, adminPassword);
            
            if (result.Succeeded)
            {
                // Assign Admin role
                await _userManager.AddToRoleAsync(adminUser, "Admin");
                
                _logger.LogInformation("Default admin user created successfully: {Email}", adminEmail);
            }
            else
            {
                _logger.LogError("Failed to create default admin user: {Errors}", 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding default admin user");
        }
    }
}