namespace MoneyTracker.Domain.Enums;

public enum AccountType
{
    Checking = 1,
    Savings = 2,
    Credit = 3,
    Investment = 4,
    Cash = 5
}

public enum TransactionType
{
    Income = 1,
    Expense = 2
}

public enum CategoryType
{
    Income = 1,
    Expense = 2
}