using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Infrastructure.Persistence;

namespace MoneyTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly MySqlDbContext? _mySqlDbContext;
    private readonly SqlServerDbContext? _sqlServerDbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<HealthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        // Try to get the appropriate DbContext based on the configured provider
        try
        {
            _mySqlDbContext = serviceProvider.GetService<MySqlDbContext>();
            _sqlServerDbContext = serviceProvider.GetService<SqlServerDbContext>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to resolve database contexts during health check initialization");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var healthStatus = new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            Version = "1.0.0"
        };

        return Ok(healthStatus);
    }

    [HttpGet("database")]
    public async Task<IActionResult> DatabaseHealth()
    {
        try
        {
            var databaseProvider = _configuration.GetValue<string>("DatabaseSettings:Provider");
            var canConnect = false;
            var databaseName = "Unknown";

            switch (databaseProvider?.ToLower())
            {
                case "mysql":
                    if (_mySqlDbContext != null)
                    {
                        canConnect = await _mySqlDbContext.Database.CanConnectAsync();
                        databaseName = "MySQL";
                    }
                    break;
                    
                case "sqlserver":
                    if (_sqlServerDbContext != null)
                    {
                        canConnect = await _sqlServerDbContext.Database.CanConnectAsync();
                        databaseName = "SQL Server";
                    }
                    break;
            }

            var healthStatus = new
            {
                Status = canConnect ? "Healthy" : "Unhealthy",
                Database = databaseName,
                Provider = databaseProvider,
                Timestamp = DateTime.UtcNow,
                CanConnect = canConnect
            };

            return canConnect ? Ok(healthStatus) : StatusCode(503, healthStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            
            var errorStatus = new
            {
                Status = "Unhealthy",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            };

            return StatusCode(503, errorStatus);
        }
    }
}