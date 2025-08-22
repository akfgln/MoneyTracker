using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Application.DTOs.Transaction;

public class TransactionResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryIcon { get; set; }
    public string? CategoryColor { get; set; }
    public decimal Amount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal VatRate { get; set; }
    public string VatRateDescription { get; set; } = string.Empty;
    public string Currency { get; set; } = "EUR";
    public DateTime TransactionDate { get; set; }
    public DateTime BookingDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? MerchantName { get; set; }
    public TransactionType TransactionType { get; set; }
    public string TransactionTypeDisplay { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentMethodDisplay { get; set; }
    public string? Location { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? ReceiptPath { get; set; }
    public bool IsRecurring { get; set; }
    public string? RecurrencePattern { get; set; }
    public Guid? RecurrenceGroupId { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedDate { get; set; }
    public string? VerifiedBy { get; set; }
    public bool IsPending { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public string? ExternalTransactionId { get; set; }
    public string? ImportSource { get; set; }
    public DateTime? ImportDate { get; set; }
    public bool IsAutoCategorized { get; set; }
    public decimal? ExchangeRate { get; set; }
    public string? OriginalCurrency { get; set; }
    public decimal? OriginalAmount { get; set; }
    public string DisplayAmount { get; set; } = string.Empty;
    public string DisplayDescription { get; set; } = string.Empty;
    public int DaysFromToday { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}