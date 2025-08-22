using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Application.DTOs.File;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Infrastructure.Services.BankParsers;

public class GermanBankStatementParser : IBankStatementParser
{
    private readonly Dictionary<string, IBankFormatParser> _bankParsers;
    private readonly ICategoryService _categoryService;
    private readonly ILogger<GermanBankStatementParser> _logger;

    public GermanBankStatementParser(
        ICategoryService categoryService,
        ILogger<GermanBankStatementParser> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
        _bankParsers = new Dictionary<string, IBankFormatParser>(StringComparer.OrdinalIgnoreCase)
        {
            { "deutsche bank", new DeutscheBankParser() },
            { "commerzbank", new CommerzbankParser() },
            { "dkb", new DkbParser() },
            { "ing", new IngParser() },
            { "sparkasse", new SparkasseParser() },
            { "postbank", new PostbankParser() },
            { "volksbank", new VolksbankParser() },
            { "raiffeisenbank", new RaiffeisenBankParser() }
        };
    }

    public async Task<List<ExtractedTransactionDto>> ParseBankStatementAsync(string text, string bankName)
    {
        _logger.LogInformation("Parsing bank statement for bank: {BankName}", bankName);
        
        var parser = GetBankParser(bankName);
        List<ExtractedTransactionDto> transactions;
        
        if (parser != null)
        {
            _logger.LogInformation("Using specific parser for {BankName}", parser.BankName);
            transactions = await parser.ParseTransactionsAsync(text);
        }
        else
        {
            _logger.LogInformation("Using generic parser for unknown bank: {BankName}", bankName);
            transactions = await ParseGenericFormat(text);
        }

        // Add category suggestions
        foreach (var transaction in transactions)
        {
            try
            {
                var suggestedCategory = await _categoryService.SuggestCategoryAsync(
                    transaction.Description,
                    transaction.MerchantName,
                    transaction.Amount,
                    transaction.TransactionType
                );

                if (suggestedCategory != null)
                {
                    transaction.SuggestedCategoryId = suggestedCategory.Id;
                    transaction.SuggestedCategoryName = suggestedCategory.DisplayName;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to suggest category for transaction: {Description}", transaction.Description);
            }
        }

        _logger.LogInformation("Successfully parsed {TransactionCount} transactions", transactions.Count);
        return transactions;
    }

    public bool SupportsBankFormat(string bankName)
    {
        return GetBankParser(bankName) != null;
    }

    public async Task<BankStatementInfo> ExtractStatementInfoAsync(string text, string bankName)
    {
        var parser = GetBankParser(bankName);
        
        if (parser != null)
        {
            return await parser.ExtractStatementInfoAsync(text);
        }
        
        return await ExtractGenericStatementInfo(text, bankName);
    }

    public List<string> GetSupportedBanks()
    {
        return _bankParsers.Values.Select(p => p.BankName).ToList();
    }

    private IBankFormatParser? GetBankParser(string bankName)
    {
        if (string.IsNullOrWhiteSpace(bankName))
            return null;

        var normalizedBankName = bankName.ToLower().Trim();

        // Direct match
        if (_bankParsers.TryGetValue(normalizedBankName, out var directParser))
            return directParser;

        // Partial match
        var partialMatch = _bankParsers.FirstOrDefault(kvp =>
            normalizedBankName.Contains(kvp.Key) || kvp.Key.Contains(normalizedBankName));

        return partialMatch.Value;
    }

    private async Task<List<ExtractedTransactionDto>> ParseGenericFormat(string text)
    {
        var transactions = new List<ExtractedTransactionDto>();
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var patterns = new[]
        {
            // German date format with amount
            @"(\d{2}\.\d{2}\.\d{4})\s+.*?(-?\d{1,3}(?:\.\d{3})*,\d{2})\s*€?\s*(.+)",
            // Alternative format with booking date
            @"(\d{2}\.\d{2}\.\d{4})\s+(\d{2}\.\d{2}\.\d{4})?\s+(.+?)\s+(-?\d{1,3}(?:\.\d{3})*,\d{2})\s*€?",
            // ISO date format
            @"(\d{4}-\d{2}-\d{2})\s+.*?(-?\d{1,3}(?:\.\d{3})*,\d{2})\s*€?\s*(.+)"
        };

        foreach (var line in lines)
        {
            foreach (var pattern in patterns)
            {
                var transaction = TryParseLineWithPattern(line, pattern);
                if (transaction != null)
                {
                    transactions.Add(transaction);
                    break;
                }
            }
        }

        return transactions;
    }

    private ExtractedTransactionDto? TryParseLineWithPattern(string line, string pattern)
    {
        try
        {
            var match = Regex.Match(line, pattern, RegexOptions.IgnoreCase);
            if (!match.Success) return null;

            DateTime transactionDate;
            DateTime? bookingDate = null;
            decimal amount;
            string description;

            if (pattern.Contains("\\d{4}-\\d{2}-\\d{2}")) // ISO format
            {
                if (!DateTime.TryParseExact(match.Groups[1].Value, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out transactionDate))
                    return null;

                var amountText = match.Groups[2].Value.Replace(".", "").Replace(",", ".");
                if (!decimal.TryParse(amountText, NumberStyles.Number,
                    CultureInfo.InvariantCulture, out amount))
                    return null;

                description = match.Groups[3].Value.Trim();
            }
            else // German format
            {
                if (!DateTime.TryParseExact(match.Groups[1].Value, "dd.MM.yyyy",
                    CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out transactionDate))
                    return null;

                // Check if there's a booking date
                if (match.Groups.Count > 4 && !string.IsNullOrEmpty(match.Groups[2].Value))
                {
                    if (DateTime.TryParseExact(match.Groups[2].Value, "dd.MM.yyyy",
                        CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out var bookingDateValue))
                    {
                        bookingDate = bookingDateValue;
                    }

                    description = match.Groups[3].Value.Trim();
                    var amountText = match.Groups[4].Value.Replace(".", "").Replace(",", ".");
                    if (!decimal.TryParse(amountText, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out amount))
                        return null;
                }
                else
                {
                    var amountText = match.Groups[2].Value.Replace(".", "").Replace(",", ".");
                    if (!decimal.TryParse(amountText, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out amount))
                        return null;

                    description = match.Groups[3].Value.Trim();
                }
            }

            var transaction = new ExtractedTransactionDto
            {
                Id = Guid.NewGuid().ToString(),
                TransactionDate = transactionDate,
                BookingDate = bookingDate ?? transactionDate,
                Amount = Math.Abs(amount),
                Description = CleanDescription(description),
                TransactionType = amount < 0 ? TransactionType.Expense : TransactionType.Income,
                TransactionTypeDisplay = amount < 0 ? "Ausgabe" : "Einnahme",
                ConfidenceScore = 0.7m // Generic parsing has lower confidence
            };

            // Try to extract merchant name and reference
            ExtractAdditionalInfo(transaction, description);

            return transaction;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to parse line with pattern {Pattern}: {Line}", pattern, line);
            return null;
        }
    }

    private void ExtractAdditionalInfo(ExtractedTransactionDto transaction, string description)
    {
        // Extract merchant name patterns
        var merchantPatterns = new[]
        {
            @"KARTENZAHLUNG\s+(\w+.*?)\s+\d{2}\.\d{2}",
            @"ELV\s+(\w+.*?)\s+\d{2}\.\d{2}",
            @"LASTSCHRIFT\s+(.*?)\s+MANDATSREF",
            @"ÜBERWEISUNG\s+(.*?)\s+(?:VERWENDUNGSZWECK|$)",
            @"GUTSCHRIFT\s+(.*?)\s+(?:VON|$)"
        };

        foreach (var pattern in merchantPatterns)
        {
            var match = Regex.Match(description, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                transaction.MerchantName = match.Groups[1].Value.Trim();
                break;
            }
        }

        // Extract reference number
        var refPatterns = new[]
        {
            @"MANDATSREF\s+(\w+)",
            @"KUNDENREF\s+(\w+)",
            @"REF\.\s+(\w+)",
            @"REFERENZ\s+(\w+)"
        };

        foreach (var pattern in refPatterns)
        {
            var match = Regex.Match(description, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                transaction.ReferenceNumber = match.Groups[1].Value.Trim();
                break;
            }
        }

        // Extract payment method
        if (description.Contains("KARTENZAHLUNG", StringComparison.OrdinalIgnoreCase))
            transaction.PaymentMethod = "Karte";
        else if (description.Contains("LASTSCHRIFT", StringComparison.OrdinalIgnoreCase))
            transaction.PaymentMethod = "Lastschrift";
        else if (description.Contains("ÜBERWEISUNG", StringComparison.OrdinalIgnoreCase))
            transaction.PaymentMethod = "Überweisung";
        else if (description.Contains("GUTSCHRIFT", StringComparison.OrdinalIgnoreCase))
            transaction.PaymentMethod = "Gutschrift";
        else if (description.Contains("DAUERAUFTRAG", StringComparison.OrdinalIgnoreCase))
            transaction.PaymentMethod = "Dauerauftrag";
    }

    private string CleanDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return string.Empty;

        // Remove common German banking artifacts
        var cleaned = description
            .Replace("KARTENZAHLUNG", "")
            .Replace("LASTSCHRIFT", "")
            .Replace("ÜBERWEISUNG", "")
            .Replace("GUTSCHRIFT", "")
            .Replace("ELV", "")
            .Replace("DAUERAUFTRAG", "")
            .Trim();

        // Remove extra whitespace
        cleaned = Regex.Replace(cleaned, @"\s+", " ");

        return cleaned;
    }

    private async Task<BankStatementInfo> ExtractGenericStatementInfo(string text, string bankName)
    {
        var info = new BankStatementInfo
        {
            BankName = bankName,
            Currency = "EUR"
        };

        // Try to extract account number
        var accountPattern = @"Konto(?:nummer)?:?\s*(\d+(?:\s+\d+)*)";   
        var accountMatch = Regex.Match(text, accountPattern, RegexOptions.IgnoreCase);
        if (accountMatch.Success)
        {
            info.AccountNumber = accountMatch.Groups[1].Value.Replace(" ", "");
        }

        // Try to extract statement period
        var periodPattern = @"(?:vom|Zeitraum):?\s*(\d{2}\.\d{2}\.\d{4})\s*(?:bis|-)\s*(\d{2}\.\d{2}\.\d{4})";
        var periodMatch = Regex.Match(text, periodPattern, RegexOptions.IgnoreCase);
        if (periodMatch.Success)
        {
            if (DateTime.TryParseExact(periodMatch.Groups[1].Value, "dd.MM.yyyy",
                CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out var startDate))
            {
                info.StatementPeriodStart = startDate;
            }

            if (DateTime.TryParseExact(periodMatch.Groups[2].Value, "dd.MM.yyyy",
                CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out var endDate))
            {
                info.StatementPeriodEnd = endDate;
            }
        }

        return info;
    }
}