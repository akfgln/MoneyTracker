using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Infrastructure.Services;

namespace MoneyTracker.Infrastructure.Persistence.Factories;

public class SqlServerDbContextFactory : IDesignTimeDbContextFactory<SqlServerDbContext>
{
    public SqlServerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SqlServerDbContext>();
        
        // Use a dummy connection string for design-time operations
        var connectionString = "Server=(localdb)\\mssqllocaldb;Database=MoneyTrackerDb_Design;Trusted_Connection=true;MultipleActiveResultSets=true;";
        
        optionsBuilder.UseSqlServer(
            connectionString,
            b => b.MigrationsAssembly(typeof(SqlServerDbContext).Assembly.FullName));

        // Create dummy services for design-time
        var currentUserService = new DesignTimeCurrentUserService();
        var dateTimeService = new DateTimeService();

        return new SqlServerDbContext(optionsBuilder.Options, currentUserService, dateTimeService);
    }
}