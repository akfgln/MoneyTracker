using System.ComponentModel;

namespace MoneyTracker.Domain.Enums;

public enum DatabaseProvider
{
    [Description("MySQL")]
    MySql = 1,
    
    [Description("SQL Server")]
    SqlServer = 2
}