using MoneyTracker.Domain.Enums;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Application.DTOs.Transaction;
using System.Globalization;

namespace MoneyTracker.Infrastructure.Services;

public class VatCalculationService : IVatCalculationService
{
    private readonly ICategoryRepository _categoryRepository;
    
    // German VAT rates as of 2025
    private readonly Dictionary<decimal, string> _germanVatRates = new()
    {
        { 0.00m, "Steuerbefreit (0%)" },
        { 0.07m, "Ermäßigter Steuersatz (7%)" },
        { 0.19m, "Regelsteuersatz (19%)" }
    };

    public VatCalculationService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public VatCalculationResult CalculateVat(decimal grossAmount, decimal vatRate, TransactionType transactionType)
    {
        if (grossAmount < 0)
            throw new ArgumentException("Bruttobetrag kann nicht negativ sein", nameof(grossAmount));
        
        if (vatRate < 0 || vatRate > 1)
            throw new ArgumentException("MwSt.-Satz muss zwischen 0 und 1 liegen", nameof(vatRate));

        var netAmount = grossAmount / (1 + vatRate);
        var vatAmount = grossAmount - netAmount;
        
        // For income transactions, VAT is typically output VAT (Umsatzsteuer)
        // For expense transactions, VAT is typically input VAT (Vorsteuer) - deductible
        var isDeductible = transactionType == TransactionType.Expense;
        var deductibleVatAmount = isDeductible ? vatAmount : 0;

        return new VatCalculationResult
        {
            GrossAmount = Math.Round(grossAmount, 2),
            NetAmount = Math.Round(netAmount, 2),
            VatAmount = Math.Round(vatAmount, 2),
            VatRate = vatRate,
            VatRateDescription = GetVatRateDescription(vatRate),
            IsDeductible = isDeductible,
            DeductibleVatAmount = Math.Round(deductibleVatAmount, 2),
            Currency = "EUR"
        };
    }

    public VatCalculationResult CalculateVatFromNet(decimal netAmount, decimal vatRate, TransactionType transactionType)
    {
        if (netAmount < 0)
            throw new ArgumentException("Nettobetrag kann nicht negativ sein", nameof(netAmount));
        
        if (vatRate < 0 || vatRate > 1)
            throw new ArgumentException("MwSt.-Satz muss zwischen 0 und 1 liegen", nameof(vatRate));

        var vatAmount = netAmount * vatRate;
        var grossAmount = netAmount + vatAmount;
        
        var isDeductible = transactionType == TransactionType.Expense;
        var deductibleVatAmount = isDeductible ? vatAmount : 0;

        return new VatCalculationResult
        {
            GrossAmount = Math.Round(grossAmount, 2),
            NetAmount = Math.Round(netAmount, 2),
            VatAmount = Math.Round(vatAmount, 2),
            VatRate = vatRate,
            VatRateDescription = GetVatRateDescription(vatRate),
            IsDeductible = isDeductible,
            DeductibleVatAmount = Math.Round(deductibleVatAmount, 2),
            Currency = "EUR"
        };
    }

    public async Task<decimal> GetDefaultVatRateAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        return category?.DefaultVatRate ?? 0.19m; // Default to German standard rate
    }

    public VatSummaryDto GetVatSummary(IEnumerable<Domain.Entities.Transaction> transactions)
    {
        var transactionList = transactions.ToList();
        
        var totalVatAmount = transactionList.Sum(t => t.VatAmount);
        var inputVatAmount = transactionList
            .Where(t => t.TransactionType == TransactionType.Expense)
            .Sum(t => t.VatAmount);
        var outputVatAmount = transactionList
            .Where(t => t.TransactionType == TransactionType.Income)
            .Sum(t => t.VatAmount);
        
        var deductibleVatAmount = inputVatAmount; // Input VAT is generally deductible
        var vatLiability = outputVatAmount - deductibleVatAmount;

        var vatRateBreakdown = transactionList
            .GroupBy(t => t.VatRate)
            .Select(g => new VatRateSummaryDto
            {
                VatRate = g.Key,
                VatRateDescription = GetVatRateDescription(g.Key),
                NetAmount = Math.Round(g.Sum(t => t.NetAmount), 2),
                VatAmount = Math.Round(g.Sum(t => t.VatAmount), 2),
                GrossAmount = Math.Round(g.Sum(t => t.Amount), 2),
                TransactionCount = g.Count()
            })
            .OrderBy(v => v.VatRate)
            .ToList();

        return new VatSummaryDto
        {
            TotalVatAmount = Math.Round(totalVatAmount, 2),
            DeductibleVatAmount = Math.Round(deductibleVatAmount, 2),
            OutputVatAmount = Math.Round(outputVatAmount, 2),
            InputVatAmount = Math.Round(inputVatAmount, 2),
            VatLiability = Math.Round(vatLiability, 2),
            VatRateBreakdown = vatRateBreakdown
        };
    }

    public Dictionary<decimal, string> GetGermanVatRates()
    {
        return new Dictionary<decimal, string>(_germanVatRates);
    }

    public bool IsValidGermanVatRate(decimal vatRate)
    {
        return _germanVatRates.ContainsKey(vatRate) || 
               (vatRate >= 0 && vatRate <= 1); // Allow custom rates within reasonable range
    }

    private string GetVatRateDescription(decimal vatRate)
    {
        if (_germanVatRates.TryGetValue(vatRate, out var description))
        {
            return description;
        }

        return $"Steuersatz ({vatRate.ToString("P1", CultureInfo.GetCultureInfo("de-DE"))})"; // German percentage format
    }
}