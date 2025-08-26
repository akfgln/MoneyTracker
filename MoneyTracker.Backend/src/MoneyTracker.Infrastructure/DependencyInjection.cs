using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Domain.Enums;
using MoneyTracker.Infrastructure.Persistence;
using MoneyTracker.Infrastructure.Services;
using MoneyTracker.Infrastructure.Services.BankParsers;
using MoneyTracker.Infrastructure.Repositories;

namespace MoneyTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTransient<IDateTime, DateTimeService>();

        var databaseProvider = configuration.GetValue<DatabaseProvider>("DatabaseSettings:Provider");
        
        switch (databaseProvider)
        {
            case DatabaseProvider.MySql:
                services.AddDbContext<MySqlDbContext>(options =>
                    options.UseMySql(
                        configuration.GetConnectionString("MySqlConnection"),
                        ServerVersion.Create(new Version(8, 0, 0), Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MySql),
                        b => b.MigrationsAssembly(typeof(MySqlDbContext).Assembly.FullName)));
                
                services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<MySqlDbContext>());
                services.AddScoped<DbContext>(provider => provider.GetRequiredService<MySqlDbContext>());
                break;
                
            case DatabaseProvider.SqlServer:
                services.AddDbContext<SqlServerDbContext>(options =>
                    options.UseSqlServer(
                        configuration.GetConnectionString("SqlServerConnection"),
                        b => b.MigrationsAssembly(typeof(SqlServerDbContext).Assembly.FullName)));
                
                services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<SqlServerDbContext>());
                services.AddScoped<DbContext>(provider => provider.GetRequiredService<SqlServerDbContext>());
                break;
                
            default:
                throw new InvalidOperationException($"Database provider '{databaseProvider}' is not supported.");
        }

        // Register repositories
        services.AddScoped<IUserRepository, MoneyTracker.Infrastructure.Repositories.UserRepository>();
        services.AddScoped<IAccountRepository, MoneyTracker.Infrastructure.Repositories.AccountRepository>();
        services.AddScoped<ICategoryRepository, MoneyTracker.Infrastructure.Repositories.CategoryRepository>();
        services.AddScoped<ITransactionRepository, MoneyTracker.Infrastructure.Repositories.TransactionRepository>();
        services.AddScoped<IUploadedFileRepository, UploadedFileRepository>();
        
        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Register services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IEmailService, FileEmailService>();
        services.AddScoped<IVatCalculationService, VatCalculationService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IPdfTextExtractionService, PdfTextExtractionService>();
        services.AddScoped<IBankStatementParser, GermanBankStatementParser>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IDuplicateDetectionService, DuplicateDetectionService>();
        services.AddScoped<IFileProcessingService, FileProcessingService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ISmartCategorizationService, SmartCategorizationService>();

        return services;
    }
}