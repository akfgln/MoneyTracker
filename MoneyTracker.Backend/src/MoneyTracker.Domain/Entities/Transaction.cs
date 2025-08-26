using MoneyTracker.Domain.Common;
using MoneyTracker.Domain.Enums;
using MoneyTracker.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Domain.Entities;

public class Transaction : BaseAuditableEntity
{
    public static Transaction CreateExpense(
        Guid userId,
        Guid accountId,
        Guid categoryId,
        decimal amount,
        DateTime transactionDate,
        string description,
        decimal vatRate = 0.19m)
    {
        var transaction = new Transaction
        {
            UserId = userId,
            AccountId = accountId,
            CategoryId = categoryId,
            Amount = Math.Round(amount, 2),
            TransactionDate = transactionDate,
            BookingDate = transactionDate,
            Description = description,
            TransactionType = TransactionType.Expense,
            VatRate = vatRate
        };

        transaction.CalculateVatAmounts();
        return transaction;
    }

    public DateTime Date { get; set; }
    public Guid UserId { get; set; }
    public Guid AccountId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal VatRate { get; set; } = 0.19m;
    public string Currency { get; set; } = "EUR";
    public DateTime TransactionDate { get; set; }
    public DateTime BookingDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? MerchantName { get; set; }
    public string? ReceiptPath { get; set; }
    public TransactionType TransactionType { get; set; }
    public string? Notes { get; set; }
    public string? ReferenceNumber { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string? Location { get; set; }
    public string? Tags { get; set; }
    public bool IsRecurring { get; set; } = false;
    public string? RecurrencePattern { get; set; }
    public Guid? RecurrenceGroupId { get; set; }
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedDate { get; set; }
    public string? VerifiedBy { get; set; }
    public bool IsPending { get; set; } = false;
    public DateTime? ProcessedDate { get; set; }
    public string? ExternalTransactionId { get; set; }
    public string? ImportSource { get; set; }
    public DateTime? ImportDate { get; set; }
    public bool IsAutoCategorized { get; set; } = false;
    public decimal? ExchangeRate { get; set; }
    public string? OriginalCurrency { get; set; }
    public decimal? OriginalAmount { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Account Account { get; set; } = null!;
    public virtual Category Category { get; set; } = null!;

    // Value object properties
    public Money AmountMoney => new Money(Amount, Currency);
    public Money NetAmountMoney => new Money(NetAmount, Currency);
    public Money VatAmountMoney => new Money(VatAmount, Currency);
    public VatRate VatRateObject => new VatRate(VatRate, GetVatRateDescription());
    public Money? OriginalAmountMoney => OriginalAmount.HasValue && !string.IsNullOrEmpty(OriginalCurrency) ? 
        new Money(OriginalAmount.Value, OriginalCurrency) : null;

    // Computed properties
    public bool IsIncome => TransactionType == TransactionType.Income;
    public bool IsExpense => TransactionType == TransactionType.Expense;
    public bool IsVatApplicable => VatRate > 0;
    public bool IsForeign => !string.IsNullOrEmpty(OriginalCurrency) && OriginalCurrency != Currency;
    public bool IsToday => TransactionDate.Date == DateTime.Today;
    public bool IsThisMonth => TransactionDate.Year == DateTime.Now.Year && TransactionDate.Month == DateTime.Now.Month;
    public bool IsThisYear => TransactionDate.Year == DateTime.Now.Year;
    public string DisplayAmount => IsIncome ? $"+{AmountMoney.ToGermanFormat()}" : $"-{AmountMoney.ToGermanFormat()}";
    public string DisplayDescription => string.IsNullOrWhiteSpace(Description) ? (MerchantName ?? "Unbekannt") : Description;
    public int DaysFromToday => (DateTime.Today - TransactionDate.Date).Days;
    
    public List<string> TagsList
    {
        get => string.IsNullOrEmpty(Tags) ? new List<string>() : 
               Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList();
        set => Tags = string.Join(",", value.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()));
    }

    // Helper methods
    public string GetVatRateDescription()
    {
        return VatRate switch
        {
            0.19m => "Regelsteuersatz (19%)",
            0.07m => "Ermäßigter Steuersatz (7%)",
            0.00m => "Steuerbefreit (0%)",
            _ => $"Steuersatz ({VatRate * 100:F1}%)"
        };
    }

    public void CalculateVatAmounts()
    {
        if (VatRate <= 0)
        {
            NetAmount = Amount;
            VatAmount = 0;
        }
        else
        {
            // Calculate from gross amount
            NetAmount = Math.Round(Amount / (1 + VatRate), 2);
            VatAmount = Math.Round(Amount - NetAmount, 2);
        }
    }

    public void SetAmountFromNet(decimal netAmount, decimal vatRate)
    {
        NetAmount = Math.Round(netAmount, 2);
        VatRate = vatRate;
        VatAmount = Math.Round(netAmount * vatRate, 2);
        Amount = NetAmount + VatAmount;
    }

    public void Verify(string verifiedBy)
    {
        IsVerified = true;
        VerifiedDate = DateTime.UtcNow;
        VerifiedBy = verifiedBy;
    }

    public void Unverify()
    {
        IsVerified = false;
        VerifiedDate = null;
        VerifiedBy = null;
    }

    public void Process()
    {
        IsPending = false;
        ProcessedDate = DateTime.UtcNow;
    }

    public void AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) return;
        
        var tags = TagsList;
        if (!tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
        {
            tags.Add(tag);
            TagsList = tags;
        }
    }

    public void RemoveTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) return;
        
        var tags = TagsList;
        tags.RemoveAll(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
        TagsList = tags;
    }

    public bool HasTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) return false;
        return TagsList.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
    }

    public void SetExchangeRate(decimal rate, string originalCurrency, decimal originalAmount)
    {
        ExchangeRate = Math.Round(rate, 6);
        OriginalCurrency = originalCurrency;
        OriginalAmount = Math.Round(originalAmount, 2);
    }

    public void ClearExchangeRate()
    {
        ExchangeRate = null;
        OriginalCurrency = null;
        OriginalAmount = null;
    }

    public string GetTransactionTypeDisplayName()
    {
        return TransactionType switch
        {
            TransactionType.Income => "Einnahme",
            TransactionType.Expense => "Ausgabe",
            _ => "Unbekannt"
        };
    }

    public string GetPaymentMethodDisplayName()
    {
        if (!PaymentMethod.HasValue) return "";
        
        return PaymentMethod switch
        {
            Enums.PaymentMethod.Cash => "Bargeld",
            Enums.PaymentMethod.CreditCard => "Karte",
            Enums.PaymentMethod.DebitCard => "EC-Karte",
            Enums.PaymentMethod.BankTransfer => "Überweisung",
            Enums.PaymentMethod.PayPal => "PayPal",
            _ => "Unbekannt"
        };
    }

    public bool IsRecent(int days = 7)
    {
        return TransactionDate >= DateTime.Today.AddDays(-days);
    }

    public bool IsInDateRange(DateTime startDate, DateTime endDate)
    {
        return TransactionDate.Date >= startDate.Date && TransactionDate.Date <= endDate.Date;
    }

    public bool MatchesSearchTerm(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) return true;
        
        var term = searchTerm.ToLowerInvariant();
        
        return Description.ToLowerInvariant().Contains(term) ||
               (MerchantName?.ToLowerInvariant().Contains(term) == true) ||
               (Notes?.ToLowerInvariant().Contains(term) == true) ||
               (ReferenceNumber?.ToLowerInvariant().Contains(term) == true) ||
               TagsList.Any(tag => tag.ToLowerInvariant().Contains(term));
    }

    public decimal GetSignedAmount()
    {
        return TransactionType == TransactionType.Income ? Amount : -Amount;
    }

    public void UpdateFromImport(string importSource, string? externalId = null)
    {
        ImportSource = importSource;
        ImportDate = DateTime.UtcNow;
        ExternalTransactionId = externalId;
        
        if (!IsVerified && !IsAutoCategorized)
        {
            // Mark as auto-categorized if it was imported
            IsAutoCategorized = true;
        }
    }

    // Static factory methods
    public static Transaction CreateIncome(
        Guid userId, 
        Guid accountId, 
        Guid categoryId, 
        decimal amount, 
        DateTime transactionDate, 
        string description,
        decimal vatRate = 0.00m)
    {
        var transaction = new Transaction
        {
            UserId = userId,
            AccountId = accountId,
            CategoryId = categoryId,
            Amount = Math.Round(amount, 2),
            TransactionDate = transactionDate,
            BookingDate = transactionDate,
            Description = description,
            TransactionType = TransactionType.Income,
            VatRate = vatRate
        };
        
        transaction.CalculateVatAmounts();
        return transaction;
    }

    public TransactionType Type { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? Supplier { get; set; }
    public virtual ICollection<UploadedFile> Attachments { get; set; } = new List<UploadedFile>();

    // Computed properties
    public bool HasVat => VatRate > 0;

    public void UpdateVatAmount()
    {
        if (VatRate > 0)
        {
            VatAmount = Math.Round(Amount * VatRate / (1 + VatRate), 2);
        }
        else
        {
            VatAmount = 0;
        }
    }
}
