using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Application.DTOs.File;

namespace MoneyTracker.Infrastructure.Services.BankParsers;

// Placeholder parsers for other banks
public class PostbankParser : IBankFormatParser
{
    public string BankName => "Postbank";
    public List<string> SupportedFormats => new() { "PDF-Kontoauszug" };

    public async Task<List<ExtractedTransactionDto>> ParseTransactionsAsync(string text)
    {
        return new List<ExtractedTransactionDto>();
    }

    public async Task<BankStatementInfo> ExtractStatementInfoAsync(string text)
    {
        return new BankStatementInfo { BankName = BankName, Currency = "EUR" };
    }
}

public class VolksbankParser : IBankFormatParser
{
    public string BankName => "Volksbank";
    public List<string> SupportedFormats => new() { "PDF-Kontoauszug" };

    public async Task<List<ExtractedTransactionDto>> ParseTransactionsAsync(string text)
    {
        return new List<ExtractedTransactionDto>();
    }

    public async Task<BankStatementInfo> ExtractStatementInfoAsync(string text)
    {
        return new BankStatementInfo { BankName = BankName, Currency = "EUR" };
    }
}

public class RaiffeisenBankParser : IBankFormatParser
{
    public string BankName => "Raiffeisenbank";
    public List<string> SupportedFormats => new() { "PDF-Kontoauszug" };

    public async Task<List<ExtractedTransactionDto>> ParseTransactionsAsync(string text)
    {
        return new List<ExtractedTransactionDto>();
    }

    public async Task<BankStatementInfo> ExtractStatementInfoAsync(string text)
    {
        return new BankStatementInfo { BankName = BankName, Currency = "EUR" };
    }
}