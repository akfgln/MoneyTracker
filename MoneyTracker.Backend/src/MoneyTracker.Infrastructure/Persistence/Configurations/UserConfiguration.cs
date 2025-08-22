using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("AspNetUsers");
        
        // Additional properties beyond IdentityUser
        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(u => u.PreferredLanguage)
            .HasMaxLength(10)
            .HasDefaultValue("de-DE");
            
        builder.Property(u => u.PreferredCurrency)
            .HasMaxLength(3)
            .HasDefaultValue("EUR");
            
        builder.Property(u => u.ProfileImagePath)
            .HasMaxLength(500);
            
        builder.Property(u => u.Address)
            .HasMaxLength(500);
            
        builder.Property(u => u.City)
            .HasMaxLength(100);
            
        builder.Property(u => u.PostalCode)
            .HasMaxLength(10);
            
        builder.Property(u => u.Country)
            .HasMaxLength(100)
            .HasDefaultValue("Germany");
            
        builder.Property(u => u.EmailConfirmationToken)
            .HasMaxLength(500);
            
        builder.Property(u => u.PasswordResetToken)
            .HasMaxLength(500);
            
        builder.Property(u => u.DeactivationReason)
            .HasMaxLength(500);
        
        // Boolean properties with defaults
        builder.Property(u => u.AcceptedTerms)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(u => u.AcceptedPrivacyPolicy)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
            
        // Audit properties
        builder.Property(u => u.CreatedAt)
            .IsRequired();
            
        builder.Property(u => u.CreatedBy)
            .HasMaxLength(100);
            
        builder.Property(u => u.UpdatedBy)
            .HasMaxLength(100);
            
        builder.Property(u => u.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(u => u.DeletedBy)
            .HasMaxLength(100);
        
        // Indexes
        builder.HasIndex(u => u.FirstName)
            .HasDatabaseName("IX_AspNetUsers_FirstName");
            
        builder.HasIndex(u => u.LastName)
            .HasDatabaseName("IX_AspNetUsers_LastName");
            
        builder.HasIndex(u => new { u.FirstName, u.LastName })
            .HasDatabaseName("IX_AspNetUsers_FirstName_LastName");
            
        builder.HasIndex(u => u.IsActive)
            .HasDatabaseName("IX_AspNetUsers_IsActive");
            
        builder.HasIndex(u => u.Country)
            .HasDatabaseName("IX_AspNetUsers_Country");
            
        builder.HasIndex(u => u.IsDeleted)
            .HasDatabaseName("IX_AspNetUsers_IsDeleted");
            
        builder.HasIndex(u => u.EmailConfirmationToken)
            .HasDatabaseName("IX_AspNetUsers_EmailConfirmationToken");
            
        builder.HasIndex(u => u.PasswordResetToken)
            .HasDatabaseName("IX_AspNetUsers_PasswordResetToken");
        
        // Navigation properties
        builder.HasMany(u => u.Accounts)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(u => u.Transactions)
            .WithOne(t => t.User)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(u => u.UserConsent)
            .WithOne(uc => uc.User)
            .HasForeignKey<UserConsent>(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Ignore computed properties
        builder.Ignore(u => u.FullName);
        builder.Ignore(u => u.DisplayName);
        builder.Ignore(u => u.IsGermanResident);
        builder.Ignore(u => u.IsMinor);
        builder.Ignore(u => u.HasValidEmailConfirmationToken);
        builder.Ignore(u => u.HasValidPasswordResetToken);
    }
}