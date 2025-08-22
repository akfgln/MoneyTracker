namespace MoneyTracker.Application.DTOs.Category;

public class CategoryUsageStatsDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AverageAmount { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public DateTime FirstTransactionDate { get; set; }
    public DateTime LastTransactionDate { get; set; }
    public List<MonthlyUsageDto> MonthlyUsage { get; set; } = new();
    public List<CategoryUsageStatsDto> SubCategoryStats { get; set; } = new();
}

public class MonthlyUsageDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int TransactionCount { get; set; }
    public decimal TotalAmount { get; set; }
}