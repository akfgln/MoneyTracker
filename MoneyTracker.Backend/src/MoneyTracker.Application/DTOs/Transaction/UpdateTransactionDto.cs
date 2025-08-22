using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Application.DTOs.Transaction;

public class UpdateTransactionDto
{
    public Guid? CategoryId { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? TransactionDate { get; set; }
    public DateTime? BookingDate { get; set; }
    public string? Description { get; set; }
    public string? MerchantName { get; set; }
    public decimal? CustomVatRate { get; set; }
    public string? Notes { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Location { get; set; }
    public List<string>? Tags { get; set; }
    public bool? IsRecurring { get; set; }
    public string? RecurrencePattern { get; set; }
    public bool? IsVerified { get; set; }
}