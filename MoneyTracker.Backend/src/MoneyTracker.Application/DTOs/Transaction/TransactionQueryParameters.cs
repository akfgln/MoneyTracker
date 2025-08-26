using MoneyTracker.Domain.Enums;
using System.ComponentModel.DataAnnotations;

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
    public bool? IsVerified { get; set; }
    public bool? IsPending { get; set; }
    public bool? IsRecurring { get; set; }
    public string? Tags { get; set; }
    public string? SortBy { get; set; } = "TransactionDate";
    public string? SortDirection { get; set; } = "desc";
    public bool IncludeDeleted { get; set; } = false;


    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public TransactionType? Type { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string? SearchTerm { get; set; }
    public bool IncludeAttachments { get; set; } = false;

    // Validation
    public bool IsValid()
    {
        if (Page < 1) return false;
        if (PageSize < 1 || PageSize > 100) return false;
        if (FromDate.HasValue && ToDate.HasValue && FromDate > ToDate) return false;
        if (MinAmount.HasValue && MaxAmount.HasValue && MinAmount > MaxAmount) return false;
        return true;
    }
}

public class TransactionDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = "#000000";
    public TransactionType Type { get; set; }
    public string TypeName => Type == TransactionType.Income ? "Einnahme" : "Ausgabe";
    public decimal? VatRate { get; set; }
    public decimal? VatAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? Supplier { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string? PaymentMethodName { get; set; }
    public string? Notes { get; set; }
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool HasAttachments { get; set; }
    public int AttachmentCount { get; set; }

    // German formatted properties
    public string AmountFormatted => Amount.ToString("C", new System.Globalization.CultureInfo("de-DE"));
    public string DateFormatted => Date.ToString("dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
    public string VatAmountFormatted => VatAmount?.ToString("C", new System.Globalization.CultureInfo("de-DE")) ?? "0,00 €";
}

