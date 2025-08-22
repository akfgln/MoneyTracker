using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTracker.Domain.Common;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        
        builder.HasKey(a => a.Id);
        
        // Required properties
        builder.Property(a => a.AccountName)
            .HasMaxLength(200)
            .IsRequired();
            
        builder.Property(a => a.BankName)
            .HasMaxLength(200)
            .IsRequired();
            
        builder.Property(a => a.Currency)
            .HasMaxLength(3)
            .IsRequired()
            .HasDefaultValue("EUR");
            
        builder.Property(a => a.AccountType)
            .HasConversion<int>()
            .IsRequired();
        
        // Decimal properties with German precision
        builder.Property(a => a.CurrentBalance)
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0);
            
        builder.Property(a => a.OverdraftLimit)
            .HasPrecision(18, 2);
            
        builder.Property(a => a.CreditLimit)
            .HasPrecision(18, 2);
        
        // Optional properties
        builder.Property(a => a.IbanValue)
            .HasMaxLength(34)
            .HasColumnName("IBAN");
            
        builder.Property(a => a.BankCode)
            .HasMaxLength(20);
            
        builder.Property(a => a.AccountNumber)
            .HasMaxLength(20);
            
        builder.Property(a => a.BIC)
            .HasMaxLength(11);
            
        builder.Property(a => a.Description)
            .HasMaxLength(500);
            
        builder.Property(a => a.Color)
            .HasMaxLength(7); // Hex color code
            
        builder.Property(a => a.Icon)
            .HasMaxLength(50);
        
        // Boolean properties with defaults
        builder.Property(a => a.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
            
        builder.Property(a => a.IncludeInTotalBalance)
            .IsRequired()
            .HasDefaultValue(true);
        
        // Indexes
        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("IX_Accounts_UserId");
            
        builder.HasIndex(a => a.IbanValue)
            .HasDatabaseName("IX_Accounts_IBAN");
            
        builder.HasIndex(a => new { a.AccountNumber, a.BankCode })
            .HasDatabaseName("IX_Accounts_AccountNumber_BankCode");
            
        builder.HasIndex(a => a.AccountType)
            .HasDatabaseName("IX_Accounts_AccountType");
            
        builder.HasIndex(a => a.IsActive)
            .HasDatabaseName("IX_Accounts_IsActive");
            
        builder.HasIndex(a => a.BankName)
            .HasDatabaseName("IX_Accounts_BankName");
        
        // Navigation properties
        builder.HasOne(a => a.User)
            .WithMany(u => u.Accounts)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(a => a.Transactions)
            .WithOne(t => t.Account)
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Ignore navigation properties that are computed
        builder.Ignore(a => a.IBAN);
        builder.Ignore(a => a.Balance);
        
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