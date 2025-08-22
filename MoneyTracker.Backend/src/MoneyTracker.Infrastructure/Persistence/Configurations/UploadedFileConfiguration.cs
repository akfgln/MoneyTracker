using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Infrastructure.Persistence.Configurations;

public class UploadedFileConfiguration : IEntityTypeConfiguration<UploadedFile>
{
    public void Configure(EntityTypeBuilder<UploadedFile> builder)
    {
        builder.ToTable("UploadedFiles");

        builder.HasKey(uf => uf.Id);

        builder.Property(uf => uf.OriginalFileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(uf => uf.StoredFileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(uf => uf.FilePath)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(uf => uf.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(uf => uf.FileType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(uf => uf.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(uf => uf.ProcessingMessage)
            .HasMaxLength(1000);

        builder.Property(uf => uf.ExtractedText)
            .HasColumnType("LONGTEXT"); // For MySQL, use TEXT for SQL Server

        builder.Property(uf => uf.ExtractedDataJson)
            .HasColumnType("LONGTEXT"); // For MySQL, use TEXT for SQL Server

        builder.Property(uf => uf.Tags)
            .HasMaxLength(500);

        builder.Property(uf => uf.BankName)
            .HasMaxLength(100);

        builder.Property(uf => uf.VirusScanResult)
            .HasMaxLength(200);

        // Relationships
        builder.HasOne(uf => uf.User)
            .WithMany()
            .HasForeignKey(uf => uf.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(uf => uf.Transaction)
            .WithMany()
            .HasForeignKey(uf => uf.TransactionId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasOne(uf => uf.Account)
            .WithMany()
            .HasForeignKey(uf => uf.AccountId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(uf => uf.UserId)
            .HasDatabaseName("IX_UploadedFiles_UserId");

        builder.HasIndex(uf => uf.FileType)
            .HasDatabaseName("IX_UploadedFiles_FileType");

        builder.HasIndex(uf => uf.Status)
            .HasDatabaseName("IX_UploadedFiles_Status");

        builder.HasIndex(uf => uf.UploadDate)
            .HasDatabaseName("IX_UploadedFiles_UploadDate");

        builder.HasIndex(uf => new { uf.UserId, uf.FileType })
            .HasDatabaseName("IX_UploadedFiles_UserId_FileType");

        builder.HasIndex(uf => uf.TransactionId)
            .HasDatabaseName("IX_UploadedFiles_TransactionId")
            .IsUnique(false);

        // Soft delete filter will be applied globally in DbContext
        builder.Property(uf => uf.IsDeleted)
            .HasDefaultValue(false);
    }
}