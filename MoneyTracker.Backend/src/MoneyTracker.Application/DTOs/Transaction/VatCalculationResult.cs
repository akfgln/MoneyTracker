namespace MoneyTracker.Application.DTOs.Transaction;

public class VatCalculationResult
{
    public decimal GrossAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal VatRate { get; set; }
    public string VatRateDescription { get; set; } = string.Empty;
    public bool IsDeductible { get; set; }
    public decimal DeductibleVatAmount { get; set; }
    public string Currency { get; set; } = "EUR";
}