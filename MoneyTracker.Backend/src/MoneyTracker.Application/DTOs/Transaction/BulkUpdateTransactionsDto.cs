namespace MoneyTracker.Application.DTOs.Transaction;

public class BulkUpdateTransactionsDto
{
    public List<Guid> TransactionIds { get; set; } = new();
    public Guid? NewCategoryId { get; set; }
    public bool? MarkAsVerified { get; set; }
    public bool? MarkAsProcessed { get; set; }
    public List<string>? AddTags { get; set; }
    public List<string>? RemoveTags { get; set; }
    public string? Notes { get; set; }
}