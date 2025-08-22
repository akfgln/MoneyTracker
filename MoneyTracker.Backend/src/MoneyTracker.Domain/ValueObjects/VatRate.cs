using MoneyTracker.Domain.Common;

namespace MoneyTracker.Domain.ValueObjects;

public class VatRate : ValueObject
{
    public decimal Rate { get; private set; }
    public string Description { get; private set; }

    private VatRate() { } // EF Core

    public VatRate(decimal rate, string description)
    {
        if (rate < 0 || rate > 1)
            throw new ArgumentException("VAT rate must be between 0 and 1", nameof(rate));
        
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty", nameof(description));
        
        Rate = Math.Round(rate, 4); // Precision for VAT rates
        Description = description;
    }

    // German VAT rates as of 2025
    public static VatRate Standard = new VatRate(0.19m, "Regelsteuersatz (19%)");
    public static VatRate Reduced = new VatRate(0.07m, "Ermäßigter Steuersatz (7%)");
    public static VatRate Zero = new VatRate(0.00m, "Steuerbefreit (0%)");
    
    public decimal CalculateVatAmount(decimal netAmount)
    {
        return Math.Round(netAmount * Rate, 2);
    }
    
    public decimal CalculateGrossAmount(decimal netAmount)
    {
        return netAmount + CalculateVatAmount(netAmount);
    }
    
    public decimal CalculateNetAmount(decimal grossAmount)
    {
        return Math.Round(grossAmount / (1 + Rate), 2);
    }

    public string ToPercentageString()
    {
        return $"{Rate * 100:F1}%";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Rate;
        yield return Description;
    }

    public override string ToString()
    {
        return $"{Description} ({ToPercentageString()})";
    }
}