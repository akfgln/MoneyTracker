using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Infrastructure.Services.Auth;

namespace MoneyTracker.Infrastructure.Services;

public class DatabaseSeederHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseSeederHostedService> _logger;

    public DatabaseSeederHostedService(
        IServiceProvider serviceProvider,
        ILogger<DatabaseSeederHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting database seeding...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var roleSeeder = scope.ServiceProvider.GetRequiredService<RoleSeederService>();

            // Seed roles and default admin user
            await roleSeeder.SeedRolesAsync();
            await roleSeeder.SeedDefaultAdminAsync();

            _logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during database seeding. This is non-critical for application startup.");
            // Don't throw - seeding failures shouldn't prevent application startup
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}