using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Application.DTOs.Transaction;

public class CreateTransactionDto
{
    public Guid AccountId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EUR";
    public DateTime TransactionDate { get; set; }
    public DateTime? BookingDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? MerchantName { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal? CustomVatRate { get; set; } // Override default category VAT rate
    public string? Notes { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Location { get; set; }
    public List<string>? Tags { get; set; }
    public bool IsRecurring { get; set; } = false;
    public string? RecurrencePattern { get; set; }
}