namespace MoneyTracker.Domain.Constants;

public static class UserRoles
{
    public const string User = "User";
    public const string Premium = "Premium"; 
    public const string Admin = "Admin";
    public const string SuperAdmin = "SuperAdmin";
    
    public static readonly string[] AllRoles = { User, Premium, Admin, SuperAdmin };
    
    public static readonly Dictionary<string, string> RoleDescriptions = new()
    {
        { User, "Standard user with basic access" },
        { Premium, "Premium user with enhanced features" },
        { Admin, "Administrator with system management access" },
        { SuperAdmin, "Super administrator with full system access" }
    };
}