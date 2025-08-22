using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        
        builder.HasKey(rt => rt.Id);
        
        // Required properties
        builder.Property(rt => rt.Token)
            .HasMaxLength(500)
            .IsRequired();
            
        builder.Property(rt => rt.UserId)
            .IsRequired();
            
        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();
            
        builder.Property(rt => rt.CreatedAt)
            .IsRequired();
            
        builder.Property(rt => rt.CreatedByIp)
            .HasMaxLength(45) // IPv6 max length
            .IsRequired();
        
        // Optional properties
        builder.Property(rt => rt.RevokedByIp)
            .HasMaxLength(45);
            
        builder.Property(rt => rt.ReplacedByToken)
            .HasMaxLength(500);
            
        builder.Property(rt => rt.ReasonRevoked)
            .HasMaxLength(200);
        
        // Indexes
        builder.HasIndex(rt => rt.Token)
            .IsUnique()
            .HasDatabaseName("IX_RefreshTokens_Token");
            
        builder.HasIndex(rt => rt.UserId)
            .HasDatabaseName("IX_RefreshTokens_UserId");
            
        builder.HasIndex(rt => rt.ExpiresAt)
            .HasDatabaseName("IX_RefreshTokens_ExpiresAt");
            
        builder.HasIndex(rt => rt.RevokedAt)
            .HasDatabaseName("IX_RefreshTokens_RevokedAt");
            
        builder.HasIndex(rt => new { rt.UserId, rt.ExpiresAt })
            .HasDatabaseName("IX_RefreshTokens_UserId_ExpiresAt");
        
        // Navigation properties
        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
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