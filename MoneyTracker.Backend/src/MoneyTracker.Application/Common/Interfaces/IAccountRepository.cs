using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Application.Common.Interfaces;

public interface IAccountRepository : IRepository<Account>
{
    Task<IEnumerable<Account>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Account>> GetActiveAccountsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Account?> GetByIbanAsync(string iban, CancellationToken cancellationToken = default);
    Task<Account?> GetByAccountNumberAsync(string accountNumber, string bankCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<Account>> GetByAccountTypeAsync(AccountType accountType, CancellationToken cancellationToken = default);
    Task<IEnumerable<Account>> GetByBankNameAsync(string bankName, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalBalanceByUserIdAsync(Guid userId, string currency = "EUR", CancellationToken cancellationToken = default);
    Task<IEnumerable<Account>> GetOverdrawnAccountsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Account>> GetAccountsNearLimitsAsync(CancellationToken cancellationToken = default);
    Task<int> GetAccountCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}