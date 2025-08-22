using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Infrastructure.Persistence.Configurations;

public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable("AspNetRoles");
        
        // Additional properties beyond IdentityRole
        builder.Property(r => r.Description)
            .HasMaxLength(500);
            
        builder.Property(r => r.CreatedAt)
            .IsRequired();
            
        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        // Indexes
        builder.HasIndex(r => r.IsActive)
            .HasDatabaseName("IX_AspNetRoles_IsActive");
    }
}