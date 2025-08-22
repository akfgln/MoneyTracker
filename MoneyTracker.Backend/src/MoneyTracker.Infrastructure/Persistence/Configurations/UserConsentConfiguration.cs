using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Infrastructure.Persistence.Configurations;

public class UserConsentConfiguration : IEntityTypeConfiguration<UserConsent>
{
    public void Configure(EntityTypeBuilder<UserConsent> builder)
    {
        builder.ToTable("UserConsents");
        
        builder.HasKey(uc => uc.Id);
        
        // Required properties
        builder.Property(uc => uc.UserId)
            .IsRequired();
            
        builder.Property(uc => uc.PrivacyPolicyVersion)
            .HasMaxLength(10)
            .IsRequired()
            .HasDefaultValue("1.0");
            
        builder.Property(uc => uc.TermsOfServiceVersion)
            .HasMaxLength(10)
            .IsRequired()
            .HasDefaultValue("1.0");
        
        // Boolean properties with defaults
        builder.Property(uc => uc.PrivacyPolicyAccepted)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(uc => uc.TermsOfServiceAccepted)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(uc => uc.MarketingEmailsConsent)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(uc => uc.DataProcessingConsent)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(uc => uc.CookieConsent)
            .IsRequired()
            .HasDefaultValue(false);
        
        // Optional properties
        builder.Property(uc => uc.ConsentIpAddress)
            .HasMaxLength(45); // IPv6 max length
            
        builder.Property(uc => uc.ConsentUserAgent)
            .HasMaxLength(500);
        
        // Indexes
        builder.HasIndex(uc => uc.UserId)
            .IsUnique()
            .HasDatabaseName("IX_UserConsents_UserId");
            
        builder.HasIndex(uc => uc.PrivacyPolicyAcceptedDate)
            .HasDatabaseName("IX_UserConsents_PrivacyPolicyAcceptedDate");
            
        builder.HasIndex(uc => uc.TermsOfServiceAcceptedDate)
            .HasDatabaseName("IX_UserConsents_TermsOfServiceAcceptedDate");
        
        // Navigation properties
        builder.HasOne(uc => uc.User)
            .WithOne(u => u.UserConsent)
            .HasForeignKey<UserConsent>(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Configure base entity properties
        ConfigureBaseEntity(builder);
    }
    
    private static void ConfigureBaseEntity<T>(EntityTypeBuilder<T> builder) where T : class
    {
        builder.Property("CreatedAt")
            .IsRequired();
            
        builder.Property("CreatedBy")
            .HasMaxLength(100);
            
        builder.Property("UpdatedAt");
        
        builder.Property("UpdatedBy")
            .HasMaxLength(100);
            
        builder.Property("IsDeleted")
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property("DeletedAt");
        
        builder.Property("DeletedBy")
            .HasMaxLength(100);
    }
}