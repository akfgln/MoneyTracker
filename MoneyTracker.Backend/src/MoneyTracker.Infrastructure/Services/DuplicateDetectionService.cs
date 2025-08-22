using Microsoft.Extensions.Logging;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Application.DTOs.File;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Infrastructure.Services;

public class DuplicateDetectionService : IDuplicateDetectionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<DuplicateDetectionService> _logger;

    public DuplicateDetectionService(
        ITransactionRepository transactionRepository,
        ILogger<DuplicateDetectionService> logger)
    {
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    public async Task<List<Transaction>> FindDuplicateTransactionsAsync(Guid userId, ExtractedTransactionDto extractedTransaction)
    {
        var dateRange = TimeSpan.FromDays(3); // Check within 3 days
        var amountTolerance = 0.01m; // 1 cent tolerance
        
        var startDate = extractedTransaction.TransactionDate.AddDays(-dateRange.Days);
        var endDate = extractedTransaction.TransactionDate.AddDays(dateRange.Days);
        
        var existingTransactions = await _transactionRepository.GetByUserIdAsync(userId);
        
        var potentialDuplicates = existingTransactions.Where(t =>
            t.TransactionDate >= startDate &&
            t.TransactionDate <= endDate &&
            Math.Abs(t.Amount - extractedTransaction.Amount) <= amountTolerance
        ).ToList();

        var duplicates = new List<Transaction>();
        
        foreach (var existing in potentialDuplicates)
        {
            var similarity = await CalculateSimilarityScoreAsync(existing, extractedTransaction);
            if (similarity >= 0.8) // 80% similarity threshold
            {
                duplicates.Add(existing);
            }
        }

        _logger.LogInformation("Found {DuplicateCount} potential duplicates for transaction: {Description}", 
            duplicates.Count, extractedTransaction.Description);
        
        return duplicates;
    }

    public async Task<bool> IsDuplicateTransactionAsync(Guid userId, ExtractedTransactionDto extractedTransaction)
    {
        var duplicates = await FindDuplicateTransactionsAsync(userId, extractedTransaction);
        return duplicates.Any();
    }

    public async Task<List<ExtractedTransactionDto>> MarkDuplicatesAsync(Guid userId, List<ExtractedTransactionDto> extractedTransactions)
    {
        foreach (var transaction in extractedTransactions)
        {
            var duplicates = await FindDuplicateTransactionsAsync(userId, transaction);
            
            if (duplicates.Any())
            {
                transaction.IsDuplicate = true;
                transaction.DuplicateTransactionId = duplicates.First().Id;
                transaction.DuplicateReason = $"Mögliches Duplikat von Transaktion vom {duplicates.First().TransactionDate:dd.MM.yyyy}";
                transaction.IsSelected = false; // Don't import duplicates by default
            }
        }

        return extractedTransactions;
    }

    public async Task<double> CalculateSimilarityScoreAsync(Transaction existingTransaction, ExtractedTransactionDto extractedTransaction)
    {
        double score = 0.0;
        int factors = 0;

        // Amount similarity (40% weight)
        var amountDifference = Math.Abs(existingTransaction.Amount - extractedTransaction.Amount);
        var amountSimilarity = Math.Max(0, 1 - (double)(amountDifference / extractedTransaction.Amount));
        score += amountSimilarity * 0.4;
        factors++;

        // Date similarity (30% weight)
        var dateDifference = Math.Abs((existingTransaction.TransactionDate - extractedTransaction.TransactionDate).TotalDays);
        var dateSimilarity = Math.Max(0, 1 - (dateDifference / 7)); // 7 days tolerance
        score += dateSimilarity * 0.3;
        factors++;

        // Description similarity (25% weight)
        var descriptionSimilarity = CalculateTextSimilarity(existingTransaction.Description, extractedTransaction.Description);
        score += descriptionSimilarity * 0.25;
        factors++;

        // Transaction type similarity (5% weight)
        var typeSimilarity = existingTransaction.TransactionType == extractedTransaction.TransactionType ? 1.0 : 0.0;
        score += typeSimilarity * 0.05;
        factors++;

        return score;
    }

    private double CalculateTextSimilarity(string text1, string text2)
    {
        if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
            return 0.0;

        text1 = text1.ToLower().Trim();
        text2 = text2.ToLower().Trim();

        if (text1 == text2)
            return 1.0;

        // Simple word-based similarity
        var words1 = text1.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        var words2 = text2.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();

        var commonWords = words1.Intersect(words2).Count();
        var totalWords = words1.Union(words2).Count();

        if (totalWords == 0)
            return 0.0;

        return (double)commonWords / totalWords;
    }
}