using System.Globalization;
using System.Text.RegularExpressions;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Application.DTOs.File;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Infrastructure.Services.BankParsers;

public class DeutscheBankParser : IBankFormatParser
{
    public string BankName => "Deutsche Bank";
    public List<string> SupportedFormats => new() { "PDF-Kontoauszug", "Online-Banking Export" };

    public async Task<List<ExtractedTransactionDto>> ParseTransactionsAsync(string text)
    {
        var transactions = new List<ExtractedTransactionDto>();
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Deutsche Bank specific pattern: Date Date Description Amount
        var transactionPattern = @"(\d{2}\.\d{2}\.\d{4})\s+(\d{2}\.\d{2}\.\d{4})\s+(.*?)\s+(-?\d{1,3}(?:\.\d{3})*,\d{2})\s*[+\-]?";
        
        // Alternative pattern for different format
        var alternativePattern = @"(\d{2}\.\d{2}\.\d{4})\s+(.*?)\s+(-?\d{1,3}(?:\.\d{3})*,\d{2})";

        foreach (var line in lines)
        {
            var transaction = TryParseTransaction(line, transactionPattern, true) ??
                            TryParseTransaction(line, alternativePattern, false);
            
            if (transaction != null)
            {
                transactions.Add(transaction);
            }
        }

        return transactions;
    }

    public async Task<BankStatementInfo> ExtractStatementInfoAsync(string text)
    {
        var info = new BankStatementInfo
        {
            BankName = BankName,
            Currency = "EUR"
        };

        // Extract account number - Deutsche Bank format
        var accountPattern = @"Konto\s*(\d{10})";
        var accountMatch = Regex.Match(text, accountPattern, RegexOptions.IgnoreCase);
        if (accountMatch.Success)
        {
            info.AccountNumber = accountMatch.Groups[1].Value;
        }

        // Extract account holder
        var holderPattern = @"Kontoinhaber[^\n]*\n([^\n]+)";
        var holderMatch = Regex.Match(text, holderPattern, RegexOptions.IgnoreCase);
        if (holderMatch.Success)
        {
            info.AccountHolder = holderMatch.Groups[1].Value.Trim();
        }

        // Extract statement period
        var periodPattern = @"Kontoauszug\s+\d+\s+vom\s+(\d{2}\.\d{2}\.\d{4})\s+bis\s+(\d{2}\.\d{2}\.\d{4})";
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

        // Extract balances
        var openingBalancePattern = @"Alter\s+Kontostand[^\n]*(-?\d{1,3}(?:\.\d{3})*,\d{2})";
        var openingMatch = Regex.Match(text, openingBalancePattern, RegexOptions.IgnoreCase);
        if (openingMatch.Success)
        {
            var balanceText = openingMatch.Groups[1].Value.Replace(".", "").Replace(",", ".");
            if (decimal.TryParse(balanceText, NumberStyles.Number, CultureInfo.InvariantCulture, out var balance))
            {
                info.OpeningBalance = balance;
            }
        }

        var closingBalancePattern = @"Neuer\s+Kontostand[^\n]*(-?\d{1,3}(?:\.\d{3})*,\d{2})";
        var closingMatch = Regex.Match(text, closingBalancePattern, RegexOptions.IgnoreCase);
        if (closingMatch.Success)
        {
            var balanceText = closingMatch.Groups[1].Value.Replace(".", "").Replace(",", ".");
            if (decimal.TryParse(balanceText, NumberStyles.Number, CultureInfo.InvariantCulture, out var balance))
            {
                info.ClosingBalance = balance;
            }
        }

        return info;
    }

    private ExtractedTransactionDto? TryParseTransaction(string line, string pattern, bool hasBookingDate)
    {
        var match = Regex.Match(line, pattern, RegexOptions.IgnoreCase);
        if (!match.Success) return null;

        try
        {
            DateTime transactionDate;
            DateTime? bookingDate = null;
            string description;
            decimal amount;

            if (hasBookingDate)
            {
                // Pattern with booking date: bookingDate transactionDate description amount
                if (!DateTime.TryParseExact(match.Groups[1].Value, "dd.MM.yyyy",
                    CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out var booking))
                    return null;

                if (!DateTime.TryParseExact(match.Groups[2].Value, "dd.MM.yyyy",
                    CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out transactionDate))
                    return null;

                bookingDate = booking;
                description = match.Groups[3].Value.Trim();
                var amountText = match.Groups[4].Value.Replace(".", "").Replace(",", ".");
                if (!decimal.TryParse(amountText, NumberStyles.Number, CultureInfo.InvariantCulture, out amount))
                    return null;
            }
            else
            {
                // Pattern without booking date: date description amount
                if (!DateTime.TryParseExact(match.Groups[1].Value, "dd.MM.yyyy",
                    CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out transactionDate))
                    return null;

                description = match.Groups[2].Value.Trim();
                var amountText = match.Groups[3].Value.Replace(".", "").Replace(",", ".");
                if (!decimal.TryParse(amountText, NumberStyles.Number, CultureInfo.InvariantCulture, out amount))
                    return null;
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
                ConfidenceScore = 0.9m // High confidence for specific bank parser
            };

            // Extract additional information
            ExtractMerchantAndReference(transaction, description);

            return transaction;
        }
        catch
        {
            return null;
        }
    }

    private void ExtractMerchantAndReference(ExtractedTransactionDto transaction, string description)
    {
        // Deutsche Bank specific merchant extraction
        var merchantPatterns = new[]
        {
            @"KARTENZAHLUNG\s+([^\d]+)\s+\d{2}\.\d{2}",
            @"LASTSCHRIFT\s+([^\n]+?)\s+(?:MANDATSREF|END-TO-END)",
            @"ÜBERWEISUNG\s+([^\n]+?)\s+(?:VERWENDUNGSZWECK|IBAN)",
            @"ELV\s+([^\d]+)\s+\d{2}\.\d{2}",
            @"GUTSCHRIFT\s+([^\n]+?)\s+(?:VON|IBAN)"
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

        // Extract reference numbers
        var refPatterns = new[]
        {
            @"MANDATSREF[:\s]+(\w+)",
            @"END-TO-END-REF[:\s]+(\w+)",
            @"KUNDENREFERENZ[:\s]+(\w+)",
            @"REF[:\s]+(\w+)"
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

        // Determine payment method
        if (description.Contains("KARTENZAHLUNG", StringComparison.OrdinalIgnoreCase))
        {
            transaction.PaymentMethod = "Kartenzahlung";
        }
        else if (description.Contains("LASTSCHRIFT", StringComparison.OrdinalIgnoreCase))
        {
            transaction.PaymentMethod = "Lastschrift";
        }
        else if (description.Contains("ÜBERWEISUNG", StringComparison.OrdinalIgnoreCase))
        {
            transaction.PaymentMethod = "Überweisung";
        }
        else if (description.Contains("GUTSCHRIFT", StringComparison.OrdinalIgnoreCase))
        {
            transaction.PaymentMethod = "Gutschrift";
        }
        else if (description.Contains("DAUERAUFTRAG", StringComparison.OrdinalIgnoreCase))
        {
            transaction.PaymentMethod = "Dauerauftrag";
        }

        // Extract location for card payments
        var locationPattern = @"KARTENZAHLUNG\s+[^\d]*\s+(\d{2}\.\d{2})\s+\d{2}:\d{2}\s+([^\n]+)";
        var locationMatch = Regex.Match(description, locationPattern, RegexOptions.IgnoreCase);
        if (locationMatch.Success)
        {
            transaction.Location = locationMatch.Groups[2].Value.Trim();
        }
    }

    private string CleanDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return string.Empty;

        // Clean up Deutsche Bank specific formatting
        var cleaned = description
            .Replace("FOLGENR.", "")
            .Replace("BLZ ", "")
            .Replace("DATUM ", "")
            .Replace("UHRZEIT ", "")
            .Trim();

        // Remove multiple spaces
        cleaned = Regex.Replace(cleaned, @"\s+", " ");

        return cleaned;
    }
}