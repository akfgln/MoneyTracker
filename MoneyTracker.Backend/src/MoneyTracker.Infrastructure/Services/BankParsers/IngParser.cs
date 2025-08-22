using System.Globalization;
using System.Text.RegularExpressions;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Application.DTOs.File;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Infrastructure.Services.BankParsers;

public class IngParser : IBankFormatParser
{
    public string BankName => "ING";
    public List<string> SupportedFormats => new() { "PDF-Kontoauszug" };

    public async Task<List<ExtractedTransactionDto>> ParseTransactionsAsync(string text)
    {
        var transactions = new List<ExtractedTransactionDto>();
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // ING pattern
        var pattern = @"(\d{2}\.\d{2}\.\d{4})\s+([^\d-]+?)\s+(-?\d{1,3}(?:\.\d{3})*,\d{2})";

        foreach (var line in lines)
        {
            var match = Regex.Match(line, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var transaction = ParseIngTransaction(match);
                if (transaction != null)
                {
                    transactions.Add(transaction);
                }
            }
        }

        return transactions;
    }

    public async Task<BankStatementInfo> ExtractStatementInfoAsync(string text)
    {
        return new BankStatementInfo
        {
            BankName = BankName,
            Currency = "EUR"
        };
    }

    private ExtractedTransactionDto? ParseIngTransaction(Match match)
    {
        try
        {
            if (!DateTime.TryParseExact(match.Groups[1].Value, "dd.MM.yyyy",
                CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out var date))
                return null;

            var description = match.Groups[2].Value.Trim();
            var amountText = match.Groups[3].Value.Replace(".", "").Replace(",", ".");
            
            if (!decimal.TryParse(amountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
                return null;

            return new ExtractedTransactionDto
            {
                Id = Guid.NewGuid().ToString(),
                TransactionDate = date,
                BookingDate = date,
                Amount = Math.Abs(amount),
                Description = description,
                TransactionType = amount < 0 ? TransactionType.Expense : TransactionType.Income,
                TransactionTypeDisplay = amount < 0 ? "Ausgabe" : "Einnahme",
                ConfidenceScore = 0.85m
            };
        }
        catch
        {
            return null;
        }
    }
}