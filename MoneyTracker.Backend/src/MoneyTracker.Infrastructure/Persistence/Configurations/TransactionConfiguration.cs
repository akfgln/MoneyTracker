using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTracker.Domain.Common;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");
        
        builder.HasKey(t => t.Id);
        
        // Required properties
        builder.Property(t => t.Description)
            .HasMaxLength(500)
            .IsRequired();
            
        builder.Property(t => t.Currency)
            .HasMaxLength(3)
            .IsRequired()
            .HasDefaultValue("EUR");
            
        builder.Property(t => t.TransactionType)
            .HasConversion<int>()
            .IsRequired();
        
        // Decimal properties with German precision
        builder.Property(t => t.Amount)
            .HasPrecision(18, 2)
            .IsRequired();
            
        builder.Property(t => t.NetAmount)
            .HasPrecision(18, 2)
            .IsRequired();
            
        builder.Property(t => t.VatAmount)
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0);
            
        builder.Property(t => t.VatRate)
            .HasPrecision(5, 4) // 0.1900
            .IsRequired()
            .HasDefaultValue(0.19m);
            
        builder.Property(t => t.ExchangeRate)
            .HasPrecision(10, 6); // 1.123456
            
        builder.Property(t => t.OriginalAmount)
            .HasPrecision(18, 2);
        
        // Date properties
        builder.Property(t => t.TransactionDate)
            .IsRequired();
            
        builder.Property(t => t.BookingDate)
            .IsRequired();
        
        // Optional string properties
        builder.Property(t => t.MerchantName)
            .HasMaxLength(200);
            
        builder.Property(t => t.ReceiptPath)
            .HasMaxLength(500);
            
        builder.Property(t => t.Notes)
            .HasMaxLength(1000);
            
        builder.Property(t => t.ReferenceNumber)
            .HasMaxLength(100);
            
        builder.Property(t => t.PaymentMethod)
            .HasMaxLength(50);
            
        builder.Property(t => t.Location)
            .HasMaxLength(200);
            
        builder.Property(t => t.Tags)
            .HasMaxLength(500);
            
        builder.Property(t => t.RecurrencePattern)
            .HasMaxLength(100);
            
        builder.Property(t => t.VerifiedBy)
            .HasMaxLength(100);
            
        builder.Property(t => t.ExternalTransactionId)
            .HasMaxLength(100);
            
        builder.Property(t => t.ImportSource)
            .HasMaxLength(50);
            
        builder.Property(t => t.OriginalCurrency)
            .HasMaxLength(3);
        
        // Boolean properties with defaults
        builder.Property(t => t.IsRecurring)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(t => t.IsVerified)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(t => t.IsPending)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(t => t.IsAutoCategorized)
            .IsRequired()
            .HasDefaultValue(false);
        
        // Indexes for performance
        builder.HasIndex(t => t.UserId)
            .HasDatabaseName("IX_Transactions_UserId");
            
        builder.HasIndex(t => t.AccountId)
            .HasDatabaseName("IX_Transactions_AccountId");
            
        builder.HasIndex(t => t.CategoryId)
            .HasDatabaseName("IX_Transactions_CategoryId");
            
        builder.HasIndex(t => t.TransactionDate)
            .HasDatabaseName("IX_Transactions_TransactionDate");
            
        builder.HasIndex(t => t.BookingDate)
            .HasDatabaseName("IX_Transactions_BookingDate");
            
        builder.HasIndex(t => t.TransactionType)
            .HasDatabaseName("IX_Transactions_TransactionType");
            
        builder.HasIndex(t => t.MerchantName)
            .HasDatabaseName("IX_Transactions_MerchantName");
            
        builder.HasIndex(t => t.PaymentMethod)
            .HasDatabaseName("IX_Transactions_PaymentMethod");
            
        builder.HasIndex(t => t.IsVerified)
            .HasDatabaseName("IX_Transactions_IsVerified");
            
        builder.HasIndex(t => t.IsPending)
            .HasDatabaseName("IX_Transactions_IsPending");
            
        builder.HasIndex(t => t.IsRecurring)
            .HasDatabaseName("IX_Transactions_IsRecurring");
            
        builder.HasIndex(t => t.RecurrenceGroupId)
            .HasDatabaseName("IX_Transactions_RecurrenceGroupId");
            
        builder.HasIndex(t => t.ExternalTransactionId)
            .HasDatabaseName("IX_Transactions_ExternalTransactionId");
            
        builder.HasIndex(t => t.ImportSource)
            .HasDatabaseName("IX_Transactions_ImportSource");
            
        builder.HasIndex(t => t.ImportDate)
            .HasDatabaseName("IX_Transactions_ImportDate");
        
        // Composite indexes for common queries
        builder.HasIndex(t => new { t.UserId, t.TransactionDate })
            .HasDatabaseName("IX_Transactions_UserId_TransactionDate");
            
        builder.HasIndex(t => new { t.AccountId, t.TransactionDate })
            .HasDatabaseName("IX_Transactions_AccountId_TransactionDate");
            
        builder.HasIndex(t => new { t.CategoryId, t.TransactionDate })
            .HasDatabaseName("IX_Transactions_CategoryId_TransactionDate");
            
        builder.HasIndex(t => new { t.UserId, t.TransactionType, t.TransactionDate })
            .HasDatabaseName("IX_Transactions_UserId_TransactionType_TransactionDate");
        
        // Navigation properties
        builder.HasOne(t => t.User)
            .WithMany(u => u.Transactions)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(t => t.Account)
            .WithMany(a => a.Transactions)
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(t => t.Category)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Ignore computed properties and value objects
        builder.Ignore(t => t.AmountMoney);
        builder.Ignore(t => t.NetAmountMoney);
        builder.Ignore(t => t.VatAmountMoney);
        builder.Ignore(t => t.VatRateObject);
        builder.Ignore(t => t.OriginalAmountMoney);
        builder.Ignore(t => t.TagsList);
        
        // Configure base entity properties
        ConfigureBaseEntity(builder);
    }
    
    private static void ConfigureBaseEntity<T>(EntityTypeBuilder<T> builder) where T : BaseEntity
    {
        builder.Property(e => e.CreatedAt)
            .IsRequired();
            
        builder.Property(e => e.CreatedBy)
            .HasMaxLength(100);
            
        builder.Property(e => e.UpdatedAt);
        
        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(100);
            
        builder.Property(e => e.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(e => e.DeletedAt);
        
        builder.Property(e => e.DeletedBy)
            .HasMaxLength(100);
        
        builder.HasIndex("IsDeleted")
            .HasDatabaseName($"IX_{typeof(T).Name}_IsDeleted");
        
        if (typeof(BaseAuditableEntity).IsAssignableFrom(typeof(T)))
        {
            builder.Property("RowVersion")
                .IsRowVersion()
                .HasColumnName("RowVersion");
        }
    }
}