using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MoneyTracker.API.Middleware;
using MoneyTracker.API.Services;
using MoneyTracker.Application;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Infrastructure;
using MoneyTracker.Infrastructure.Services.Auth;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/moneytracker-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MoneyTracker API",
        Version = "v1",
        Description = "German Money Tracker API with dual database support"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure ASP.NET Core Identity with German password requirements
var databaseProvider = builder.Configuration.GetValue<string>("DatabaseSettings:Provider")?.ToLower();

var identityBuilder = builder.Services.AddIdentity<User, ApplicationRole>(options =>
{
    // German password requirements
    options.Password.RequiredLength = 12;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredUniqueChars = 6;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // Set to true to enforce email confirmation
    
    // Lockout settings (German standards)
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
});

// Conditionally add EntityFramework stores based on database provider
if (databaseProvider == "mysql")
{
    identityBuilder.AddEntityFrameworkStores<MoneyTracker.Infrastructure.Persistence.MySqlDbContext>();
}
else
{
    identityBuilder.AddEntityFrameworkStores<MoneyTracker.Infrastructure.Persistence.SqlServerDbContext>();
}

identityBuilder.AddDefaultTokenProviders();

// Add Authentication Services
builder.Services.AddScoped<IJwtService, MoneyTracker.Infrastructure.Services.JwtService>();
builder.Services.AddScoped<IEmailService, MoneyTracker.Infrastructure.Services.FileEmailService>();
builder.Services.AddScoped<IGdprService, MoneyTracker.Infrastructure.Services.GdprService>();
builder.Services.AddScoped<MoneyTracker.Infrastructure.Services.Auth.RoleSeederService>();

// Add database seeding hosted service
builder.Services.AddHostedService<MoneyTracker.Infrastructure.Services.DatabaseSeederHostedService>();

// Add Authentication with JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"];
    
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Set to true in production
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = jwtSettings.GetValue<bool>("ValidateIssuer"),
        ValidateAudience = jwtSettings.GetValue<bool>("ValidateAudience"),
        ValidateLifetime = jwtSettings.GetValue<bool>("ValidateLifetime"),
        ValidateIssuerSigningKey = jwtSettings.GetValue<bool>("ValidateIssuerSigningKey"),
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey ?? string.Empty)),
        RequireExpirationTime = jwtSettings.GetValue<bool>("RequireExpirationTime"),
        ClockSkew = TimeSpan.FromMinutes(jwtSettings.GetValue<int>("ClockSkewMinutes"))
    };
});

// Add Authorization with custom policies
builder.Services.AddAuthorization(options =>
{
    // Role-based policies
    options.AddPolicy("RequireUser", policy => 
        policy.RequireRole("User", "Premium", "Admin"));
    options.AddPolicy("RequirePremium", policy => 
        policy.RequireRole("Premium", "Admin"));
    options.AddPolicy("RequireAdmin", policy => 
        policy.RequireRole("Admin"));
        
    // Additional policies can be added here
    options.AddPolicy("RequireEmailConfirmed", policy =>
        policy.RequireClaim("email_verified", "true"));
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add Current User Service
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Add Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add Health Checks
var healthChecksBuilder = builder.Services.AddHealthChecks();

switch (databaseProvider)
{
    case "mysql":
        healthChecksBuilder.AddMySql(
            builder.Configuration.GetConnectionString("MySqlConnection") ?? string.Empty,
            name: "mysql-db");
        break;
        
    case "sqlserver":
        healthChecksBuilder.AddSqlServer(
            builder.Configuration.GetConnectionString("SqlServerConnection") ?? string.Empty,
            name: "sqlserver-db");
        break;
}

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MoneyTracker API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at apps root
    });
}

// Add custom middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

try
{
    Log.Information("Starting MoneyTracker API");
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}