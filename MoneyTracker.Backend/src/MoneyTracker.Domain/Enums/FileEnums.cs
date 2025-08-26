namespace MoneyTracker.Domain.Enums;

public enum FileType
{
    Receipt = 1,
    BankStatement = 2,
    Document = 3,
    Invoice = 4,
    Contract = 5
}

public enum FileStatus
{
    Uploaded = 1,
    Processing = 2,
    Processed = 3,
    Failed = 4,
    Imported = 5,
    VirusDetected = 6
}

public enum PaymentMethod
{
    Cash = 1,
    BankTransfer = 2,
    CreditCard = 3,
    DebitCard = 4,
    PayPal = 5,
    Check = 6,
    Other = 99
}