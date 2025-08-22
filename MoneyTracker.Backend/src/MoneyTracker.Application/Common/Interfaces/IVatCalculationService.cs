using MoneyTracker.Domain.Enums;
using MoneyTracker.Application.DTOs.Transaction;

namespace MoneyTracker.Application.Common.Interfaces;

public interface IVatCalculationService
{
    /// <summary>
    /// Calculates VAT from gross amount (amount including VAT)
    /// </summary>
    VatCalculationResult CalculateVat(decimal grossAmount, decimal vatRate, TransactionType transactionType);
    
    /// <summary>
    /// Calculates VAT from net amount (amount excluding VAT)
    /// </summary>
    VatCalculationResult CalculateVatFromNet(decimal netAmount, decimal vatRate, TransactionType transactionType);
    
    /// <summary>
    /// Gets the default VAT rate for a category
    /// </summary>
    Task<decimal> GetDefaultVatRateAsync(Guid categoryId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets VAT summary for a list of transactions
    /// </summary>
    VatSummaryDto GetVatSummary(IEnumerable<Domain.Entities.Transaction> transactions);
    
    /// <summary>
    /// Gets German VAT rates and descriptions
    /// </summary>
    Dictionary<decimal, string> GetGermanVatRates();
    
    /// <summary>
    /// Validates if VAT rate is valid for German tax system
    /// </summary>
    bool IsValidGermanVatRate(decimal vatRate);
}