namespace MoneyTracker.Domain.Common;

public abstract class BaseAuditableEntity : BaseEntity
{
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}