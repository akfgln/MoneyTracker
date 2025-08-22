using Microsoft.EntityFrameworkCore;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Infrastructure.Persistence;

namespace MoneyTracker.Infrastructure.Repositories;

public class UploadedFileRepository : IUploadedFileRepository
{
    private readonly IApplicationDbContext _context;

    public UploadedFileRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UploadedFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UploadedFiles
            .Include(f => f.User)
            .Include(f => f.Transaction)
            .Include(f => f.Account)
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted, cancellationToken);
    }

    public async Task<List<UploadedFile>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UploadedFiles
            .Include(f => f.Transaction)
            .Include(f => f.Account)
            .Where(f => f.UserId == userId && !f.IsDeleted)
            .OrderByDescending(f => f.UploadDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UploadedFile>> GetReceiptsByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.UploadedFiles
            .Include(f => f.Transaction)
            .Where(f => f.UserId == userId && f.FileType == Domain.Enums.FileType.Receipt && !f.IsDeleted)
            .OrderByDescending(f => f.UploadDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UploadedFile>> GetStatementsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UploadedFiles
            .Include(f => f.Account)
            .Where(f => f.UserId == userId && f.FileType == Domain.Enums.FileType.BankStatement && !f.IsDeleted)
            .OrderByDescending(f => f.UploadDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<UploadedFile?> GetByUserIdAndFileIdAsync(Guid userId, Guid fileId, CancellationToken cancellationToken = default)
    {
        return await _context.UploadedFiles
            .Include(f => f.Transaction)
            .Include(f => f.Account)
            .FirstOrDefaultAsync(f => f.Id == fileId && f.UserId == userId && !f.IsDeleted, cancellationToken);
    }

    public async Task AddAsync(UploadedFile file, CancellationToken cancellationToken = default)
    {
        await _context.UploadedFiles.AddAsync(file, cancellationToken);
    }

    public void Update(UploadedFile file)
    {
        _context.UploadedFiles.Update(file);
    }

    public void Delete(UploadedFile file)
    {
        file.IsDeleted = true;
        file.UpdatedAt = DateTime.UtcNow;
        _context.UploadedFiles.Update(file);
    }

    public async Task<int> GetReceiptCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UploadedFiles
            .CountAsync(f => f.UserId == userId && f.FileType == Domain.Enums.FileType.Receipt && !f.IsDeleted, cancellationToken);
    }

    public async Task<List<UploadedFile>> GetFilesByTransactionIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        return await _context.UploadedFiles
            .Where(f => f.TransactionId == transactionId && !f.IsDeleted)
            .OrderByDescending(f => f.UploadDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasDuplicateFileAsync(Guid userId, string fileName, long fileSize, CancellationToken cancellationToken = default)
    {
        return await _context.UploadedFiles
            .AnyAsync(f => f.UserId == userId && 
                          f.OriginalFileName == fileName && 
                          f.FileSize == fileSize && 
                          !f.IsDeleted &&
                          f.UploadDate > DateTime.UtcNow.AddHours(-1), cancellationToken); // Check for duplicates within 1 hour
    }
}