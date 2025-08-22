using MoneyTracker.Domain.Common;
using MoneyTracker.Domain.Enums;
using MoneyTracker.Domain.ValueObjects;

namespace MoneyTracker.Domain.Entities;

public class Account : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string? IbanValue { get; set; }
    public string BankName { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public string Currency { get; set; } = "EUR";
    public AccountType AccountType { get; set; }
    public string? BankCode { get; set; }
    public string? AccountNumber { get; set; }
    public string? BIC { get; set; }
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IncludeInTotalBalance { get; set; } = true;
    public decimal? OverdraftLimit { get; set; }
    public decimal? CreditLimit { get; set; }
    public DateTime? LastSyncDate { get; set; }
    public DateTime? ClosedDate { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    // Value object properties
    public IBAN? IBAN 
    { 
        get => !string.IsNullOrEmpty(IbanValue) ? new IBAN(IbanValue) : null;
        set => IbanValue = value?.Value;
    }

    public Money Balance => new Money(CurrentBalance, Currency);

    // Computed properties
    public string DisplayName => string.IsNullOrWhiteSpace(AccountName) ? BankName : AccountName;
    public bool IsOverdrawn => CurrentBalance < 0;
    public bool IsNearOverdraftLimit => OverdraftLimit.HasValue && CurrentBalance < (OverdraftLimit.Value * 0.1m);
    public bool IsNearCreditLimit => CreditLimit.HasValue && CurrentBalance > (CreditLimit.Value * 0.9m);
    public decimal AvailableBalance
    {
        get
        {
            if (AccountType == AccountType.Credit && CreditLimit.HasValue)
                return CreditLimit.Value - CurrentBalance;
            
            if (OverdraftLimit.HasValue)
                return CurrentBalance + OverdraftLimit.Value;
            
            return CurrentBalance;
        }
    }

    // Helper methods
    public void UpdateBalance(decimal newBalance)
    {
        CurrentBalance = Math.Round(newBalance, 2);
        LastSyncDate = DateTime.UtcNow;
    }

    public void AddTransaction(decimal amount)
    {
        CurrentBalance = Math.Round(CurrentBalance + amount, 2);
    }

    public void SubtractTransaction(decimal amount)
    {
        CurrentBalance = Math.Round(CurrentBalance - amount, 2);
    }

    public bool CanWithdraw(decimal amount)
    {
        if (AccountType == AccountType.Credit)
            return CreditLimit == null || (CurrentBalance + amount) <= CreditLimit.Value;
        
        var availableAmount = OverdraftLimit.HasValue ? 
            CurrentBalance + OverdraftLimit.Value : CurrentBalance;
        
        return availableAmount >= amount;
    }

    public void CloseAccount()
    {
        IsActive = false;
        ClosedDate = DateTime.UtcNow;
    }

    public void ReopenAccount()
    {
        IsActive = true;
        ClosedDate = null;
    }

    public string GetAccountTypeDisplayName()
    {
        return AccountType switch
        {
            AccountType.Checking => "Girokonto",
            AccountType.Savings => "Sparkonto",
            AccountType.Credit => "Kreditkonto",
            AccountType.Investment => "Anlagekonto",
            AccountType.Cash => "Bargeld",
            _ => "Unbekannt"
        };
    }
}