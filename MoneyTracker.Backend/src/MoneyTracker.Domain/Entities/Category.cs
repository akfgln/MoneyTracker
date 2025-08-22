using MoneyTracker.Domain.Common;
using MoneyTracker.Domain.Enums;
using MoneyTracker.Domain.ValueObjects;

namespace MoneyTracker.Domain.Entities;

public class Category : BaseAuditableEntity
{
    public Guid? UserId { get; set; } // Null for system categories, set for user-defined categories
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CategoryType CategoryType { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public decimal DefaultVatRate { get; set; } = 0.19m; // German standard VAT rate
    public bool IsSystemCategory { get; set; } = false;
    public string? Keywords { get; set; } // For auto-categorization
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
    public string? NameGerman { get; set; }
    public string? DescriptionGerman { get; set; }
    public bool IsDefault { get; set; } = false;
    public decimal? BudgetLimit { get; set; }
    public string? BudgetCurrency { get; set; } = "EUR";

    // Navigation properties
    public virtual User? User { get; set; }
    public virtual Category? ParentCategory { get; set; }
    public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    // Value object properties
    public VatRate VatRate => new VatRate(DefaultVatRate, GetVatRateDescription());
    public Money? BudgetLimitMoney => BudgetLimit.HasValue ? new Money(BudgetLimit.Value, BudgetCurrency!) : null;

    // Computed properties
    public string DisplayName => !string.IsNullOrEmpty(NameGerman) ? NameGerman : Name;
    public string DisplayDescription => !string.IsNullOrEmpty(DescriptionGerman) ? DescriptionGerman : Description ?? "";
    public bool IsParentCategory => ParentCategoryId == null;
    public bool IsSubCategory => ParentCategoryId != null;
    public int Level => IsParentCategory ? 0 : 1;
    public string FullPath => IsSubCategory && ParentCategory != null ? 
        $"{ParentCategory.DisplayName} > {DisplayName}" : DisplayName;
    
    // Helper methods
    public string GetVatRateDescription()
    {
        return DefaultVatRate switch
        {
            0.19m => "Regelsteuersatz (19%)",
            0.07m => "Ermäßigter Steuersatz (7%)",
            0.00m => "Steuerbefreit (0%)",
            _ => $"Steuersatz ({DefaultVatRate * 100:F1}%)"
        };
    }

    public bool HasSubCategories()
    {
        return SubCategories.Any();
    }

    public bool CanBeDeleted()
    {
        return !IsSystemCategory && !Transactions.Any() && !HasSubCategories();
    }

    public void Deactivate()
    {
        IsActive = false;
        
        // Deactivate all subcategories
        foreach (var subCategory in SubCategories)
        {
            subCategory.Deactivate();
        }
    }

    public void Activate()
    {
        IsActive = true;
        
        // If this is a subcategory, ensure parent is also active
        if (ParentCategory != null && !ParentCategory.IsActive)
        {
            ParentCategory.Activate();
        }
    }

    public bool MatchesKeywords(string text)
    {
        if (string.IsNullOrWhiteSpace(Keywords) || string.IsNullOrWhiteSpace(text))
            return false;
        
        var keywords = Keywords.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(k => k.Trim().ToLowerInvariant());
        
        var searchText = text.ToLowerInvariant();
        
        return keywords.Any(keyword => searchText.Contains(keyword));
    }

    public void UpdateBudgetLimit(decimal? limit, string currency = "EUR")
    {
        BudgetLimit = limit.HasValue ? Math.Round(limit.Value, 2) : null;
        BudgetCurrency = currency;
    }

    public decimal CalculateCurrentSpending(DateTime fromDate, DateTime toDate)
    {
        return Transactions
            .Where(t => t.TransactionDate >= fromDate && t.TransactionDate <= toDate)
            .Where(t => t.TransactionType == TransactionType.Expense)
            .Sum(t => t.Amount);
    }

    public bool IsOverBudget(DateTime fromDate, DateTime toDate)
    {
        if (!BudgetLimit.HasValue) return false;
        
        var currentSpending = CalculateCurrentSpending(fromDate, toDate);
        return currentSpending > BudgetLimit.Value;
    }

    public decimal GetBudgetUsagePercentage(DateTime fromDate, DateTime toDate)
    {
        if (!BudgetLimit.HasValue || BudgetLimit.Value == 0) return 0;
        
        var currentSpending = CalculateCurrentSpending(fromDate, toDate);
        return Math.Round((currentSpending / BudgetLimit.Value) * 100, 2);
    }

    public string GetCategoryTypeDisplayName()
    {
        return CategoryType switch
        {
            CategoryType.Income => "Einnahmen",
            CategoryType.Expense => "Ausgaben",
            _ => "Unbekannt"
        };
    }

    public static List<Category> GetGermanDefaultIncomeCategories()
    {
        return new List<Category>
        {
            new() { Name = "Salary/Wages", NameGerman = "Gehalt/Lohn", CategoryType = CategoryType.Income, DefaultVatRate = 0.00m, IsSystemCategory = true, IsDefault = true, SortOrder = 1 },
            new() { Name = "Freelance Income", NameGerman = "Freiberufliche Einkünfte", CategoryType = CategoryType.Income, DefaultVatRate = 0.19m, IsSystemCategory = true, IsDefault = true, SortOrder = 2 },
            new() { Name = "Investment Returns", NameGerman = "Kapitalerträge", CategoryType = CategoryType.Income, DefaultVatRate = 0.00m, IsSystemCategory = true, IsDefault = true, SortOrder = 3 },
            new() { Name = "Rental Income", NameGerman = "Mieteinnahmen", CategoryType = CategoryType.Income, DefaultVatRate = 0.00m, IsSystemCategory = true, IsDefault = true, SortOrder = 4 },
            new() { Name = "Business Income", NameGerman = "Geschäftseinkünfte", CategoryType = CategoryType.Income, DefaultVatRate = 0.19m, IsSystemCategory = true, IsDefault = true, SortOrder = 5 },
            new() { Name = "Other Income", NameGerman = "Sonstige Einnahmen", CategoryType = CategoryType.Income, DefaultVatRate = 0.00m, IsSystemCategory = true, IsDefault = true, SortOrder = 99 }
        };
    }

    public static List<Category> GetGermanDefaultExpenseCategories()
    {
        return new List<Category>
        {
            new() { Name = "Housing", NameGerman = "Wohnen", CategoryType = CategoryType.Expense, DefaultVatRate = 0.00m, IsSystemCategory = true, IsDefault = true, SortOrder = 1 },
            new() { Name = "Transportation", NameGerman = "Transport", CategoryType = CategoryType.Expense, DefaultVatRate = 0.19m, IsSystemCategory = true, IsDefault = true, SortOrder = 2 },
            new() { Name = "Food & Dining", NameGerman = "Essen & Trinken", CategoryType = CategoryType.Expense, DefaultVatRate = 0.07m, IsSystemCategory = true, IsDefault = true, SortOrder = 3 },
            new() { Name = "Healthcare", NameGerman = "Gesundheit", CategoryType = CategoryType.Expense, DefaultVatRate = 0.00m, IsSystemCategory = true, IsDefault = true, SortOrder = 4 },
            new() { Name = "Entertainment", NameGerman = "Unterhaltung", CategoryType = CategoryType.Expense, DefaultVatRate = 0.19m, IsSystemCategory = true, IsDefault = true, SortOrder = 5 },
            new() { Name = "Shopping", NameGerman = "Einkaufen", CategoryType = CategoryType.Expense, DefaultVatRate = 0.19m, IsSystemCategory = true, IsDefault = true, SortOrder = 6 },
            new() { Name = "Education", NameGerman = "Bildung", CategoryType = CategoryType.Expense, DefaultVatRate = 0.00m, IsSystemCategory = true, IsDefault = true, SortOrder = 7 },
            new() { Name = "Business Expenses", NameGerman = "Geschäftsausgaben", CategoryType = CategoryType.Expense, DefaultVatRate = 0.19m, IsSystemCategory = true, IsDefault = true, SortOrder = 8 },
            new() { Name = "Insurance", NameGerman = "Versicherungen", CategoryType = CategoryType.Expense, DefaultVatRate = 0.00m, IsSystemCategory = true, IsDefault = true, SortOrder = 9 },
            new() { Name = "Other Expenses", NameGerman = "Sonstige Ausgaben", CategoryType = CategoryType.Expense, DefaultVatRate = 0.19m, IsSystemCategory = true, IsDefault = true, SortOrder = 99 }
        };
    }
}