using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Application.DTOs.Transaction;

public class SummaryQueryParameters
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? AccountId { get; set; }
    public List<Guid>? AccountIds { get; set; }
    public Guid? CategoryId { get; set; }
    public List<Guid>? CategoryIds { get; set; }
    public TransactionType? TransactionType { get; set; }
    public string? Currency { get; set; } = "EUR";
    public bool GroupByCategory { get; set; } = false;
    public bool GroupByAccount { get; set; } = false;
    public bool GroupByMonth { get; set; } = false;
    public bool IncludeVatBreakdown { get; set; } = true;
}