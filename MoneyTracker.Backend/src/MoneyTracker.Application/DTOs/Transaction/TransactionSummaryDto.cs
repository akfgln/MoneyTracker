namespace MoneyTracker.Application.DTOs.Transaction;

public class TransactionSummaryDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetAmount { get; set; }
    public decimal TotalVatAmount { get; set; }
    public decimal DeductibleVatAmount { get; set; }
    public string Currency { get; set; } = "EUR";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int TransactionCount { get; set; }
    public decimal AverageTransactionAmount { get; set; }
    public decimal LargestIncome { get; set; }
    public decimal LargestExpense { get; set; }
    public List<CategorySummaryDto> CategorySummaries { get; set; } = new();
    public List<AccountSummaryDto> AccountSummaries { get; set; } = new();
    public List<MonthlySummaryDto> MonthlySummaries { get; set; } = new();
    public VatSummaryDto VatSummary { get; set; } = new();
}

public class CategorySummaryDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal VatAmount { get; set; }
    public int TransactionCount { get; set; }
    public decimal Percentage { get; set; }
}

public class AccountSummaryDto
{
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetAmount { get; set; }
    public int TransactionCount { get; set; }
}

public class MonthlySummaryDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetAmount { get; set; }
    public int TransactionCount { get; set; }
}

public class VatSummaryDto
{
    public decimal TotalVatAmount { get; set; }
    public decimal DeductibleVatAmount { get; set; }
    public decimal OutputVatAmount { get; set; }
    public decimal InputVatAmount { get; set; }
    public decimal VatLiability { get; set; }
    public List<VatRateSummaryDto> VatRateBreakdown { get; set; } = new();
}

public class VatRateSummaryDto
{
    public decimal VatRate { get; set; }
    public string VatRateDescription { get; set; } = string.Empty;
    public decimal NetAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal GrossAmount { get; set; }
    public int TransactionCount { get; set; }
}