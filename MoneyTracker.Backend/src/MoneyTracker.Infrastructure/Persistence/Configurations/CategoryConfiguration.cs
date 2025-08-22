using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTracker.Domain.Common;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        
        builder.HasKey(c => c.Id);
        
        // Required properties
        builder.Property(c => c.Name)
            .HasMaxLength(200)
            .IsRequired();
            
        builder.Property(c => c.CategoryType)
            .HasConversion<int>()
            .IsRequired();
            
        builder.Property(c => c.DefaultVatRate)
            .HasPrecision(5, 4) // 0.1900
            .IsRequired()
            .HasDefaultValue(0.19m);
        
        // Optional properties
        builder.Property(c => c.Description)
            .HasMaxLength(500);
            
        builder.Property(c => c.NameGerman)
            .HasMaxLength(200);
            
        builder.Property(c => c.DescriptionGerman)
            .HasMaxLength(500);
            
        builder.Property(c => c.Icon)
            .HasMaxLength(50);
            
        builder.Property(c => c.Color)
            .HasMaxLength(7); // Hex color code
            
        builder.Property(c => c.Keywords)
            .HasMaxLength(1000);
            
        builder.Property(c => c.BudgetCurrency)
            .HasMaxLength(3)
            .HasDefaultValue("EUR");
        
        // Decimal properties
        builder.Property(c => c.BudgetLimit)
            .HasPrecision(18, 2);
        
        // Boolean properties with defaults
        builder.Property(c => c.IsSystemCategory)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
            
        builder.Property(c => c.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);
        
        // Integer properties
        builder.Property(c => c.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);
        
        // Indexes
        builder.HasIndex(c => c.CategoryType)
            .HasDatabaseName("IX_Categories_CategoryType");
            
        builder.HasIndex(c => c.ParentCategoryId)
            .HasDatabaseName("IX_Categories_ParentCategoryId");
            
        builder.HasIndex(c => new { c.Name, c.CategoryType })
            .HasDatabaseName("IX_Categories_Name_CategoryType");
            
        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("IX_Categories_IsActive");
            
        builder.HasIndex(c => c.IsSystemCategory)
            .HasDatabaseName("IX_Categories_IsSystemCategory");
            
        builder.HasIndex(c => c.IsDefault)
            .HasDatabaseName("IX_Categories_IsDefault");
            
        builder.HasIndex(c => c.SortOrder)
            .HasDatabaseName("IX_Categories_SortOrder");
        
        // Self-referencing relationship
        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Navigation properties
        builder.HasMany(c => c.Transactions)
            .WithOne(t => t.Category)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Ignore computed properties
        builder.Ignore(c => c.VatRate);
        builder.Ignore(c => c.BudgetLimitMoney);
        
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