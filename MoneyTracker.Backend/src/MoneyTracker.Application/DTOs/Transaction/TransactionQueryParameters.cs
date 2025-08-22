using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Application.DTOs.Transaction;

public class TransactionQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? CategoryId { get; set; }
    public List<Guid>? CategoryIds { get; set; }
    public Guid? AccountId { get; set; }
    public List<Guid>? AccountIds { get; set; }
    public TransactionType? TransactionType { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? MerchantName { get; set; }
    public string? PaymentMethod { get; set; }
    public bool? IsVerified { get; set; }
    public bool? IsPending { get; set; }
    public bool? IsRecurring { get; set; }
    public string? Tags { get; set; }
    public string? SortBy { get; set; } = "TransactionDate";
    public string? SortDirection { get; set; } = "desc";
    public bool IncludeDeleted { get; set; } = false;
}