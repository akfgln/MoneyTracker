using Microsoft.EntityFrameworkCore;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Infrastructure.Repositories;

public class AccountRepository : Repository<Account>, IAccountRepository
{
    public AccountRepository(DbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Account>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.AccountName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetActiveAccountsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.UserId == userId && a.IsActive)
            .OrderBy(a => a.AccountName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Account?> GetByIbanAsync(string iban, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(a => a.IbanValue == iban, cancellationToken);
    }

    public async Task<Account?> GetByAccountNumberAsync(string accountNumber, string bankCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(
            a => a.AccountNumber == accountNumber && a.BankCode == bankCode, 
            cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetByAccountTypeAsync(AccountType accountType, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.AccountType == accountType)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetByBankNameAsync(string bankName, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.BankName.Contains(bankName))
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalBalanceByUserIdAsync(Guid userId, string currency = "EUR", CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.UserId == userId && a.IsActive && a.IncludeInTotalBalance && a.Currency == currency)
            .SumAsync(a => a.CurrentBalance, cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetOverdrawnAccountsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.IsActive && a.CurrentBalance < 0)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetAccountsNearLimitsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.IsActive && 
                   ((a.AccountType == AccountType.Credit && a.CreditLimit.HasValue && 
                     a.CurrentBalance > a.CreditLimit.Value * 0.9m) ||
                    (a.OverdraftLimit.HasValue && 
                     a.CurrentBalance < (a.OverdraftLimit.Value * 0.1m))))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetAccountCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(a => a.UserId == userId && a.IsActive, cancellationToken);
    }
}