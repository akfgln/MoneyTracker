using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Infrastructure.Services;

namespace MoneyTracker.Infrastructure.Persistence.Factories;

public class MySqlDbContextFactory : IDesignTimeDbContextFactory<MySqlDbContext>
{
    public MySqlDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MySqlDbContext>();
        
        // Use a dummy connection string for design-time operations
        var connectionString = "Server=localhost;Port=3306;Database=MoneyTrackerDb_Design;Uid=root;Pwd=design;";
        
        // Use a fixed server version to avoid connection during design-time
        optionsBuilder.UseMySql(
            connectionString,
            ServerVersion.Create(new Version(8, 0, 0), Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MySql),
            b => b.MigrationsAssembly(typeof(MySqlDbContext).Assembly.FullName));

        // Create dummy services for design-time
        var currentUserService = new DesignTimeCurrentUserService();
        var dateTimeService = new DateTimeService();

        return new MySqlDbContext(optionsBuilder.Options, currentUserService, dateTimeService);
    }
}

public class DesignTimeCurrentUserService : ICurrentUserService
{
    public string? UserId => "design-time-user";
    public string? UserName => "Design Time User";
}