using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Application.DTOs.Transaction;

public class TransactionSearchDto
{
    public string? Query { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<Guid>? CategoryIds { get; set; }
    public List<Guid>? AccountIds { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public TransactionType? TransactionType { get; set; }
    public string? MerchantName { get; set; }
    public string? PaymentMethod { get; set; }
    public List<string>? Tags { get; set; }
    public bool? IsVerified { get; set; }
    public bool? IsPending { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}