using AutoMapper;
using Microsoft.Extensions.Logging;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Application.Common.Models;
using MoneyTracker.Application.Common.Exceptions;
using MoneyTracker.Application.DTOs.Transaction;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Enums;
using System.Globalization;

namespace MoneyTracker.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IVatCalculationService _vatCalculationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<TransactionService> _logger;
    private readonly ICurrentUserService _currentUserService;

    public TransactionService(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository,
        IAccountRepository accountRepository,
        IVatCalculationService vatCalculationService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<TransactionService> logger,
        ICurrentUserService currentUserService)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _accountRepository = accountRepository;
        _vatCalculationService = vatCalculationService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TransactionResponseDto> CreateTransactionAsync(Guid userId, CreateTransactionDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new transaction for user {UserId}", userId);

        // Validate account belongs to user
        var account = await _accountRepository.GetByIdAsync(dto.AccountId, cancellationToken);
        if (account?.UserId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to access account {AccountId}", userId, dto.AccountId);
            throw new UnauthorizedAccessException("Konto nicht gefunden oder nicht zugänglich");
        }

        // Validate category exists
        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId, cancellationToken);
        if (category == null)
        {
            _logger.LogWarning("Category {CategoryId} not found", dto.CategoryId);
            throw new NotFoundException(nameof(Category), dto.CategoryId);
        }

        // Calculate VAT
        var vatRate = dto.CustomVatRate ?? category.DefaultVatRate;
        var vatCalculation = _vatCalculationService.CalculateVat(dto.Amount, vatRate, dto.TransactionType);

        // Create transaction
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AccountId = dto.AccountId,
            CategoryId = dto.CategoryId,
            Amount = vatCalculation.GrossAmount,
            NetAmount = vatCalculation.NetAmount,
            VatAmount = vatCalculation.VatAmount,
            VatRate = vatRate,
            Currency = dto.Currency,
            TransactionDate = dto.TransactionDate,
            BookingDate = dto.BookingDate ?? dto.TransactionDate,
            Description = dto.Description,
            MerchantName = dto.MerchantName,
            TransactionType = dto.TransactionType,
            Notes = dto.Notes,
            ReferenceNumber = dto.ReferenceNumber,
            PaymentMethod = dto.PaymentMethod,
            Location = dto.Location,
            IsRecurring = dto.IsRecurring,
            RecurrencePattern = dto.RecurrencePattern
        };

        if (dto.Tags?.Any() == true)
        {
            transaction.TagsList = dto.Tags;
        }

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Transaction {TransactionId} created successfully for user {UserId}", transaction.Id, userId);

        return await MapToResponseDtoAsync(transaction, cancellationToken);
    }

    public async Task<PagedResult<TransactionResponseDto>> GetTransactionsAsync(Guid userId, TransactionQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting transactions for user {UserId} with parameters: Page={Page}, PageSize={PageSize}", 
            userId, parameters.Page, parameters.PageSize);

        var query = await BuildTransactionQueryAsync(userId, parameters, cancellationToken);
        
        var totalCount = query.Count();
        var transactions = query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToList();

        var transactionDtos = new List<TransactionResponseDto>();
        foreach (var transaction in transactions)
        {
            transactionDtos.Add(await MapToResponseDtoAsync(transaction, cancellationToken));
        }

        return PagedResult<TransactionResponseDto>.Create(transactionDtos, totalCount, parameters.Page, parameters.PageSize);
    }

    public async Task<TransactionResponseDto?> GetTransactionByIdAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId, cancellationToken);
        
        if (transaction == null || transaction.UserId != userId)
        {
            _logger.LogWarning("Transaction {TransactionId} not found or not accessible by user {UserId}", transactionId, userId);
            return null;
        }

        return await MapToResponseDtoAsync(transaction, cancellationToken);
    }

    public async Task<TransactionResponseDto?> UpdateTransactionAsync(Guid userId, Guid transactionId, UpdateTransactionDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating transaction {TransactionId} for user {UserId}", transactionId, userId);

        var transaction = await _transactionRepository.GetByIdAsync(transactionId, cancellationToken);
        
        if (transaction == null || transaction.UserId != userId)
        {
            _logger.LogWarning("Transaction {TransactionId} not found or not accessible by user {UserId}", transactionId, userId);
            return null;
        }

        // Update properties if provided
        if (dto.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId.Value, cancellationToken);
            if (category == null)
            {
                throw new NotFoundException(nameof(Category), dto.CategoryId.Value);
            }
            transaction.CategoryId = dto.CategoryId.Value;
        }

        if (dto.Amount.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(transaction.CategoryId, cancellationToken);
            var vatRate = dto.CustomVatRate ?? category?.DefaultVatRate ?? transaction.VatRate;
            var vatCalculation = _vatCalculationService.CalculateVat(dto.Amount.Value, vatRate, transaction.TransactionType);
            
            transaction.Amount = vatCalculation.GrossAmount;
            transaction.NetAmount = vatCalculation.NetAmount;
            transaction.VatAmount = vatCalculation.VatAmount;
            transaction.VatRate = vatRate;
        }

        if (dto.TransactionDate.HasValue)
            transaction.TransactionDate = dto.TransactionDate.Value;
        
        if (dto.BookingDate.HasValue)
            transaction.BookingDate = dto.BookingDate.Value;
        
        if (dto.Description != null)
            transaction.Description = dto.Description;
        
        if (dto.MerchantName != null)
            transaction.MerchantName = dto.MerchantName;
        
        if (dto.Notes != null)
            transaction.Notes = dto.Notes;
        
        if (dto.ReferenceNumber != null)
            transaction.ReferenceNumber = dto.ReferenceNumber;
        
        if (dto.PaymentMethod != null)
            transaction.PaymentMethod = dto.PaymentMethod;
        
        if (dto.Location != null)
            transaction.Location = dto.Location;
        
        if (dto.Tags != null)
            transaction.TagsList = dto.Tags;
        
        if (dto.IsRecurring.HasValue)
            transaction.IsRecurring = dto.IsRecurring.Value;
        
        if (dto.RecurrencePattern != null)
            transaction.RecurrencePattern = dto.RecurrencePattern;
        
        if (dto.IsVerified.HasValue && dto.IsVerified.Value)
        {
            transaction.Verify(_currentUserService.UserId?.ToString() ?? "System");
        }
        else if (dto.IsVerified.HasValue && !dto.IsVerified.Value)
        {
            transaction.Unverify();
        }

        _transactionRepository.Update(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Transaction {TransactionId} updated successfully", transactionId);

        return await MapToResponseDtoAsync(transaction, cancellationToken);
    }

    public async Task<bool> DeleteTransactionAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting transaction {TransactionId} for user {UserId}", transactionId, userId);

        var transaction = await _transactionRepository.GetByIdAsync(transactionId, cancellationToken);
        
        if (transaction == null || transaction.UserId != userId)
        {
            _logger.LogWarning("Transaction {TransactionId} not found or not accessible by user {UserId}", transactionId, userId);
            return false;
        }

        _transactionRepository.Delete(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Transaction {TransactionId} deleted successfully", transactionId);
        return true;
    }

    public async Task<List<TransactionResponseDto>> SearchTransactionsAsync(Guid userId, TransactionSearchDto searchDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Searching transactions for user {UserId} with query: {Query}", userId, searchDto.Query);

        var transactions = await _transactionRepository.GetByUserIdAsync(userId, cancellationToken);
        var query = transactions.AsQueryable();

        // Apply search filters
        if (!string.IsNullOrWhiteSpace(searchDto.Query))
        {
            var searchTerm = searchDto.Query.ToLowerInvariant();
            query = query.Where(t => t.MatchesSearchTerm(searchTerm));
        }

        if (searchDto.StartDate.HasValue)
            query = query.Where(t => t.TransactionDate >= searchDto.StartDate.Value);

        if (searchDto.EndDate.HasValue)
            query = query.Where(t => t.TransactionDate <= searchDto.EndDate.Value);

        if (searchDto.CategoryIds?.Any() == true)
            query = query.Where(t => searchDto.CategoryIds.Contains(t.CategoryId));

        if (searchDto.AccountIds?.Any() == true)
            query = query.Where(t => searchDto.AccountIds.Contains(t.AccountId));

        if (searchDto.MinAmount.HasValue)
            query = query.Where(t => t.Amount >= searchDto.MinAmount.Value);

        if (searchDto.MaxAmount.HasValue)
            query = query.Where(t => t.Amount <= searchDto.MaxAmount.Value);

        if (searchDto.TransactionType.HasValue)
            query = query.Where(t => t.TransactionType == searchDto.TransactionType.Value);

        if (!string.IsNullOrWhiteSpace(searchDto.MerchantName))
            query = query.Where(t => t.MerchantName != null && t.MerchantName.ToLowerInvariant().Contains(searchDto.MerchantName.ToLowerInvariant()));

        if (!string.IsNullOrWhiteSpace(searchDto.PaymentMethod))
            query = query.Where(t => t.PaymentMethod != null && t.PaymentMethod.ToLowerInvariant().Contains(searchDto.PaymentMethod.ToLowerInvariant()));

        if (searchDto.Tags?.Any() == true)
            query = query.Where(t => searchDto.Tags.Any(tag => t.HasTag(tag)));

        if (searchDto.IsVerified.HasValue)
            query = query.Where(t => t.IsVerified == searchDto.IsVerified.Value);

        if (searchDto.IsPending.HasValue)
            query = query.Where(t => t.IsPending == searchDto.IsPending.Value);

        var results = query
            .OrderByDescending(t => t.TransactionDate)
            .Skip((searchDto.Page - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize)
            .ToList();

        var dtos = new List<TransactionResponseDto>();
        foreach (var transaction in results)
        {
            dtos.Add(await MapToResponseDtoAsync(transaction, cancellationToken));
        }

        return dtos;
    }

    public async Task<bool> BulkUpdateTransactionsAsync(Guid userId, BulkUpdateTransactionsDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Bulk updating {Count} transactions for user {UserId}", dto.TransactionIds.Count, userId);

        var transactions = new List<Transaction>();
        
        foreach (var transactionId in dto.TransactionIds)
        {
            var transaction = await _transactionRepository.GetByIdAsync(transactionId, cancellationToken);
            if (transaction != null && transaction.UserId == userId)
            {
                transactions.Add(transaction);
            }
        }

        if (!transactions.Any())
        {
            _logger.LogWarning("No valid transactions found for bulk update");
            return false;
        }

        foreach (var transaction in transactions)
        {
            if (dto.NewCategoryId.HasValue)
            {
                var category = await _categoryRepository.GetByIdAsync(dto.NewCategoryId.Value, cancellationToken);
                if (category != null)
                {
                    transaction.CategoryId = dto.NewCategoryId.Value;
                    // Recalculate VAT with new category's default rate
                    var vatCalculation = _vatCalculationService.CalculateVat(transaction.Amount, category.DefaultVatRate, transaction.TransactionType);
                    transaction.NetAmount = vatCalculation.NetAmount;
                    transaction.VatAmount = vatCalculation.VatAmount;
                    transaction.VatRate = category.DefaultVatRate;
                }
            }

            if (dto.MarkAsVerified.HasValue && dto.MarkAsVerified.Value)
            {
                transaction.Verify(_currentUserService.UserId?.ToString() ?? "System");
            }
            else if (dto.MarkAsVerified.HasValue && !dto.MarkAsVerified.Value)
            {
                transaction.Unverify();
            }

            if (dto.MarkAsProcessed.HasValue && dto.MarkAsProcessed.Value)
            {
                transaction.Process();
            }

            if (dto.AddTags?.Any() == true)
            {
                foreach (var tag in dto.AddTags)
                {
                    transaction.AddTag(tag);
                }
            }

            if (dto.RemoveTags?.Any() == true)
            {
                foreach (var tag in dto.RemoveTags)
                {
                    transaction.RemoveTag(tag);
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Notes))
            {
                transaction.Notes = dto.Notes;
            }
        }

        _transactionRepository.UpdateRange(transactions);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully bulk updated {Count} transactions", transactions.Count);
        return true;
    }

    public async Task<TransactionSummaryDto> GetTransactionSummaryAsync(Guid userId, SummaryQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting transaction summary for user {UserId}", userId);

        var transactions = await _transactionRepository.GetByUserIdAsync(userId, cancellationToken);
        var query = transactions.AsQueryable();

        // Apply filters
        if (parameters.StartDate.HasValue)
            query = query.Where(t => t.TransactionDate >= parameters.StartDate.Value);

        if (parameters.EndDate.HasValue)
            query = query.Where(t => t.TransactionDate <= parameters.EndDate.Value);

        if (parameters.AccountId.HasValue)
            query = query.Where(t => t.AccountId == parameters.AccountId.Value);

        if (parameters.AccountIds?.Any() == true)
            query = query.Where(t => parameters.AccountIds.Contains(t.AccountId));

        if (parameters.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == parameters.CategoryId.Value);

        if (parameters.CategoryIds?.Any() == true)
            query = query.Where(t => parameters.CategoryIds.Contains(t.CategoryId));

        if (parameters.TransactionType.HasValue)
            query = query.Where(t => t.TransactionType == parameters.TransactionType.Value);

        var filteredTransactions = query.ToList();
        
        // Calculate summary statistics
        var totalIncome = filteredTransactions.Where(t => t.TransactionType == TransactionType.Income).Sum(t => t.Amount);
        var totalExpenses = filteredTransactions.Where(t => t.TransactionType == TransactionType.Expense).Sum(t => t.Amount);
        var netAmount = totalIncome - totalExpenses;
        
        var summary = new TransactionSummaryDto
        {
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            NetAmount = netAmount,
            TotalVatAmount = filteredTransactions.Sum(t => t.VatAmount),
            DeductibleVatAmount = filteredTransactions.Where(t => t.TransactionType == TransactionType.Expense).Sum(t => t.VatAmount),
            Currency = parameters.Currency ?? "EUR",
            StartDate = parameters.StartDate,
            EndDate = parameters.EndDate,
            TransactionCount = filteredTransactions.Count,
            AverageTransactionAmount = filteredTransactions.Any() ? filteredTransactions.Average(t => t.Amount) : 0,
            LargestIncome = filteredTransactions.Where(t => t.TransactionType == TransactionType.Income).DefaultIfEmpty().Max(t => t?.Amount ?? 0),
            LargestExpense = filteredTransactions.Where(t => t.TransactionType == TransactionType.Expense).DefaultIfEmpty().Max(t => t?.Amount ?? 0)
        };

        // Generate VAT summary if requested
        if (parameters.IncludeVatBreakdown)
        {
            summary.VatSummary = _vatCalculationService.GetVatSummary(filteredTransactions);
        }

        // Group by category if requested
        if (parameters.GroupByCategory)
        {
            summary.CategorySummaries = await GenerateCategorySummariesAsync(filteredTransactions, cancellationToken);
        }

        // Group by account if requested
        if (parameters.GroupByAccount)
        {
            summary.AccountSummaries = await GenerateAccountSummariesAsync(filteredTransactions, cancellationToken);
        }

        // Group by month if requested
        if (parameters.GroupByMonth)
        {
            summary.MonthlySummaries = GenerateMonthlySummaries(filteredTransactions);
        }

        return summary;
    }

    public async Task<TransactionResponseDto?> DuplicateTransactionAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Duplicating transaction {TransactionId} for user {UserId}", transactionId, userId);

        var originalTransaction = await _transactionRepository.GetByIdAsync(transactionId, cancellationToken);
        
        if (originalTransaction == null || originalTransaction.UserId != userId)
        {
            _logger.LogWarning("Transaction {TransactionId} not found or not accessible by user {UserId}", transactionId, userId);
            return null;
        }

        var duplicatedTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AccountId = originalTransaction.AccountId,
            CategoryId = originalTransaction.CategoryId,
            Amount = originalTransaction.Amount,
            NetAmount = originalTransaction.NetAmount,
            VatAmount = originalTransaction.VatAmount,
            VatRate = originalTransaction.VatRate,
            Currency = originalTransaction.Currency,
            TransactionDate = DateTime.Today, // Use today's date for the duplicate
            BookingDate = DateTime.Today,
            Description = $"Kopie: {originalTransaction.Description}",
            MerchantName = originalTransaction.MerchantName,
            TransactionType = originalTransaction.TransactionType,
            Notes = originalTransaction.Notes,
            PaymentMethod = originalTransaction.PaymentMethod,
            Location = originalTransaction.Location,
            Tags = originalTransaction.Tags,
            IsRecurring = originalTransaction.IsRecurring,
            RecurrencePattern = originalTransaction.RecurrencePattern
        };

        await _transactionRepository.AddAsync(duplicatedTransaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Transaction {OriginalTransactionId} duplicated as {NewTransactionId}", transactionId, duplicatedTransaction.Id);

        return await MapToResponseDtoAsync(duplicatedTransaction, cancellationToken);
    }

    public async Task<bool> UpdateTransactionCategoryAsync(Guid userId, Guid transactionId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId, cancellationToken);
        
        if (transaction == null || transaction.UserId != userId)
        {
            return false;
        }

        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException(nameof(Category), categoryId);
        }

        transaction.CategoryId = categoryId;
        
        // Recalculate VAT with new category's default rate
        var vatCalculation = _vatCalculationService.CalculateVat(transaction.Amount, category.DefaultVatRate, transaction.TransactionType);
        transaction.NetAmount = vatCalculation.NetAmount;
        transaction.VatAmount = vatCalculation.VatAmount;
        transaction.VatRate = category.DefaultVatRate;

        _transactionRepository.Update(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<List<TransactionResponseDto>> GetRecentTransactionsAsync(Guid userId, int count = 10, CancellationToken cancellationToken = default)
    {
        var transactions = await _transactionRepository.GetRecentByUserIdAsync(userId, count, cancellationToken);
        
        var dtos = new List<TransactionResponseDto>();
        foreach (var transaction in transactions)
        {
            dtos.Add(await MapToResponseDtoAsync(transaction, cancellationToken));
        }

        return dtos;
    }

    public async Task<bool> VerifyTransactionAsync(Guid userId, Guid transactionId, string verifiedBy, CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId, cancellationToken);
        
        if (transaction == null || transaction.UserId != userId)
        {
            return false;
        }

        transaction.Verify(verifiedBy);
        
        _transactionRepository.Update(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ProcessPendingTransactionAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId, cancellationToken);
        
        if (transaction == null || transaction.UserId != userId)
        {
            return false;
        }

        transaction.Process();
        
        _transactionRepository.Update(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // Helper methods
    private async Task<IQueryable<Transaction>> BuildTransactionQueryAsync(Guid userId, TransactionQueryParameters parameters, CancellationToken cancellationToken)
    {
        var transactions = await _transactionRepository.GetByUserIdAsync(userId, cancellationToken);
        var query = transactions.AsQueryable();

        // Apply filters
        if (parameters.StartDate.HasValue)
            query = query.Where(t => t.TransactionDate >= parameters.StartDate.Value);

        if (parameters.EndDate.HasValue)
            query = query.Where(t => t.TransactionDate <= parameters.EndDate.Value);

        if (parameters.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == parameters.CategoryId.Value);

        if (parameters.CategoryIds?.Any() == true)
            query = query.Where(t => parameters.CategoryIds.Contains(t.CategoryId));

        if (parameters.AccountId.HasValue)
            query = query.Where(t => t.AccountId == parameters.AccountId.Value);

        if (parameters.AccountIds?.Any() == true)
            query = query.Where(t => parameters.AccountIds.Contains(t.AccountId));

        if (parameters.TransactionType.HasValue)
            query = query.Where(t => t.TransactionType == parameters.TransactionType.Value);

        if (parameters.MinAmount.HasValue)
            query = query.Where(t => t.Amount >= parameters.MinAmount.Value);

        if (parameters.MaxAmount.HasValue)
            query = query.Where(t => t.Amount <= parameters.MaxAmount.Value);

        if (!string.IsNullOrWhiteSpace(parameters.MerchantName))
            query = query.Where(t => t.MerchantName != null && t.MerchantName.ToLowerInvariant().Contains(parameters.MerchantName.ToLowerInvariant()));

        if (!string.IsNullOrWhiteSpace(parameters.PaymentMethod))
            query = query.Where(t => t.PaymentMethod != null && t.PaymentMethod.ToLowerInvariant().Contains(parameters.PaymentMethod.ToLowerInvariant()));

        if (parameters.IsVerified.HasValue)
            query = query.Where(t => t.IsVerified == parameters.IsVerified.Value);

        if (parameters.IsPending.HasValue)
            query = query.Where(t => t.IsPending == parameters.IsPending.Value);

        if (parameters.IsRecurring.HasValue)
            query = query.Where(t => t.IsRecurring == parameters.IsRecurring.Value);

        if (!string.IsNullOrWhiteSpace(parameters.Tags))
        {
            var tags = parameters.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim());
            query = query.Where(t => tags.Any(tag => t.HasTag(tag)));
        }

        // Apply sorting
        query = parameters.SortBy?.ToLower() switch
        {
            "amount" => parameters.SortDirection?.ToLower() == "asc" 
                ? query.OrderBy(t => t.Amount) 
                : query.OrderByDescending(t => t.Amount),
            "description" => parameters.SortDirection?.ToLower() == "asc" 
                ? query.OrderBy(t => t.Description) 
                : query.OrderByDescending(t => t.Description),
            "merchantname" => parameters.SortDirection?.ToLower() == "asc" 
                ? query.OrderBy(t => t.MerchantName) 
                : query.OrderByDescending(t => t.MerchantName),
            "bookingdate" => parameters.SortDirection?.ToLower() == "asc" 
                ? query.OrderBy(t => t.BookingDate) 
                : query.OrderByDescending(t => t.BookingDate),
            _ => parameters.SortDirection?.ToLower() == "asc" 
                ? query.OrderBy(t => t.TransactionDate) 
                : query.OrderByDescending(t => t.TransactionDate)
        };

        return query;
    }

    private async Task<TransactionResponseDto> MapToResponseDtoAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(transaction.CategoryId, cancellationToken);
        var account = await _accountRepository.GetByIdAsync(transaction.AccountId, cancellationToken);

        return new TransactionResponseDto
        {
            Id = transaction.Id,
            UserId = transaction.UserId,
            AccountId = transaction.AccountId,
            AccountName = account?.DisplayName ?? "Unbekannt",
            CategoryId = transaction.CategoryId,
            CategoryName = category?.DisplayName ?? "Unbekannt",
            CategoryIcon = category?.Icon,
            CategoryColor = category?.Color,
            Amount = transaction.Amount,
            NetAmount = transaction.NetAmount,
            VatAmount = transaction.VatAmount,
            VatRate = transaction.VatRate,
            VatRateDescription = transaction.GetVatRateDescription(),
            Currency = transaction.Currency,
            TransactionDate = transaction.TransactionDate,
            BookingDate = transaction.BookingDate,
            Description = transaction.Description,
            MerchantName = transaction.MerchantName,
            TransactionType = transaction.TransactionType,
            TransactionTypeDisplay = transaction.GetTransactionTypeDisplayName(),
            Notes = transaction.Notes,
            ReferenceNumber = transaction.ReferenceNumber,
            PaymentMethod = transaction.PaymentMethod,
            PaymentMethodDisplay = transaction.GetPaymentMethodDisplayName(),
            Location = transaction.Location,
            Tags = transaction.TagsList,
            ReceiptPath = transaction.ReceiptPath,
            IsRecurring = transaction.IsRecurring,
            RecurrencePattern = transaction.RecurrencePattern,
            RecurrenceGroupId = transaction.RecurrenceGroupId,
            IsVerified = transaction.IsVerified,
            VerifiedDate = transaction.VerifiedDate,
            VerifiedBy = transaction.VerifiedBy,
            IsPending = transaction.IsPending,
            ProcessedDate = transaction.ProcessedDate,
            ExternalTransactionId = transaction.ExternalTransactionId,
            ImportSource = transaction.ImportSource,
            ImportDate = transaction.ImportDate,
            IsAutoCategorized = transaction.IsAutoCategorized,
            ExchangeRate = transaction.ExchangeRate,
            OriginalCurrency = transaction.OriginalCurrency,
            OriginalAmount = transaction.OriginalAmount,
            DisplayAmount = transaction.DisplayAmount,
            DisplayDescription = transaction.DisplayDescription,
            DaysFromToday = transaction.DaysFromToday,
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt ?? transaction.CreatedAt
        };
    }

    private async Task<List<CategorySummaryDto>> GenerateCategorySummariesAsync(List<Transaction> transactions, CancellationToken cancellationToken)
    {
        var categoryGroups = transactions.GroupBy(t => t.CategoryId);
        var summaries = new List<CategorySummaryDto>();
        var totalAmount = transactions.Sum(t => Math.Abs(t.Amount));

        foreach (var group in categoryGroups)
        {
            var category = await _categoryRepository.GetByIdAsync(group.Key, cancellationToken);
            var categoryTotal = group.Sum(t => Math.Abs(t.Amount));
            
            summaries.Add(new CategorySummaryDto
            {
                CategoryId = group.Key,
                CategoryName = category?.DisplayName ?? "Unbekannt",
                TotalAmount = categoryTotal,
                VatAmount = group.Sum(t => t.VatAmount),
                TransactionCount = group.Count(),
                Percentage = totalAmount > 0 ? (categoryTotal / totalAmount) * 100 : 0
            });
        }

        return summaries.OrderByDescending(s => s.TotalAmount).ToList();
    }

    private async Task<List<AccountSummaryDto>> GenerateAccountSummariesAsync(List<Transaction> transactions, CancellationToken cancellationToken)
    {
        var accountGroups = transactions.GroupBy(t => t.AccountId);
        var summaries = new List<AccountSummaryDto>();

        foreach (var group in accountGroups)
        {
            var account = await _accountRepository.GetByIdAsync(group.Key, cancellationToken);
            var incomeTransactions = group.Where(t => t.TransactionType == TransactionType.Income);
            var expenseTransactions = group.Where(t => t.TransactionType == TransactionType.Expense);
            
            var totalIncome = incomeTransactions.Sum(t => t.Amount);
            var totalExpenses = expenseTransactions.Sum(t => t.Amount);
            
            summaries.Add(new AccountSummaryDto
            {
                AccountId = group.Key,
                AccountName = account?.DisplayName ?? "Unbekannt",
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                NetAmount = totalIncome - totalExpenses,
                TransactionCount = group.Count()
            });
        }

        return summaries.OrderByDescending(s => s.NetAmount).ToList();
    }

    private List<MonthlySummaryDto> GenerateMonthlySummaries(List<Transaction> transactions)
    {
        var monthGroups = transactions.GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month });
        var summaries = new List<MonthlySummaryDto>();

        foreach (var group in monthGroups)
        {
            var incomeTransactions = group.Where(t => t.TransactionType == TransactionType.Income);
            var expenseTransactions = group.Where(t => t.TransactionType == TransactionType.Expense);
            
            var totalIncome = incomeTransactions.Sum(t => t.Amount);
            var totalExpenses = expenseTransactions.Sum(t => t.Amount);
            
            summaries.Add(new MonthlySummaryDto
            {
                Year = group.Key.Year,
                Month = group.Key.Month,
                MonthName = CultureInfo.GetCultureInfo("de-DE").DateTimeFormat.GetMonthName(group.Key.Month),
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                NetAmount = totalIncome - totalExpenses,
                TransactionCount = group.Count()
            });
        }

        return summaries.OrderBy(s => s.Year).ThenBy(s => s.Month).ToList();
    }
}