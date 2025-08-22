using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Domain.Enums;
using System.Security;
using System.Text;

namespace MoneyTracker.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly string _uploadPath;
    private readonly Dictionary<string, string> _contentTypes;

    public LocalFileStorageService(IConfiguration configuration, ILogger<LocalFileStorageService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _uploadPath = _configuration["FileStorage:UploadPath"] ?? "uploads";
        
        EnsureDirectoryExists(_uploadPath);
        
        _contentTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { ".pdf", "application/pdf" },
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" },
            { ".gif", "image/gif" },
            { ".doc", "application/msword" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".xls", "application/vnd.ms-excel" },
            { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            { ".csv", "text/csv" },
            { ".txt", "text/plain" }
        };
    }

    public async Task<string> StoreFileAsync(byte[] fileContent, string fileName, FileType fileType, Guid userId)
    {
        try
        {
            _logger.LogInformation("Storing file {FileName} for user {UserId}", fileName, userId);
            
            // Perform virus scan
            await PerformVirusScanAsync(fileContent);
            
            var secureFileName = await GenerateSecureFileName(fileName, userId);
            var typeFolder = Path.Combine(_uploadPath, fileType.ToString().ToLower());
            EnsureDirectoryExists(typeFolder);
            
            var userFolder = Path.Combine(typeFolder, userId.ToString());
            EnsureDirectoryExists(userFolder);
            
            var fullPath = Path.Combine(userFolder, secureFileName);
            
            await File.WriteAllBytesAsync(fullPath, fileContent);
            
            _logger.LogInformation("File stored successfully at {FilePath}", fullPath);
            return fullPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store file {FileName} for user {UserId}", fileName, userId);
            throw;
        }
    }

    public async Task<byte[]> GetFileAsync(string filePath)
    {
        try
        {
            if (!await FileExistsAsync(filePath))
            {
                throw new FileNotFoundException($"Datei nicht gefunden: {filePath}");
            }
            
            return await File.ReadAllBytesAsync(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read file {FilePath}", filePath);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            if (await FileExistsAsync(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted: {FilePath}", filePath);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file {FilePath}", filePath);
            return false;
        }
    }

    public async Task<string> GenerateSecureFileName(string originalFileName, Guid userId)
    {
        var extension = Path.GetExtension(originalFileName);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var randomId = Guid.NewGuid().ToString("N")[..8];
        
        return $"{timestamp}_{randomId}{extension}";
    }

    public async Task PerformVirusScanAsync(byte[] fileContent)
    {
        // Mock virus scan implementation
        // In production, integrate with actual antivirus solution like ClamAV
        
        await Task.Delay(100); // Simulate scan time
        
        // Basic checks for potentially malicious content
        var content = Encoding.UTF8.GetString(fileContent.Take(Math.Min(fileContent.Length, 10000)).ToArray());
        
        var maliciousPatterns = new[]
        {
            "<script",
            "javascript:",
            "vbscript:",
            "onload=",
            "onerror=",
            "eval(",
            "document.write",
            "malicious_pattern", // Test pattern
            "virus_signature"    // Test pattern
        };

        foreach (var pattern in maliciousPatterns)
        {
            if (content.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Potential malicious content detected in file");
                throw new SecurityException("Potentiell schädlicher Inhalt in der Datei erkannt.");
            }
        }

        // Check file size limits
        if (fileContent.Length > 10 * 1024 * 1024) // 10MB
        {
            throw new ArgumentException("Datei ist zu groß. Maximale Größe: 10 MB");
        }

        // Additional checks can be added here
        _logger.LogDebug("Virus scan completed successfully for file of size {FileSize} bytes", fileContent.Length);
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        return File.Exists(filePath);
    }

    public async Task<long> GetFileSizeAsync(string filePath)
    {
        if (!await FileExistsAsync(filePath))
        {
            return 0;
        }
        
        var fileInfo = new FileInfo(filePath);
        return fileInfo.Length;
    }

    public string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        
        if (_contentTypes.TryGetValue(extension, out var contentType))
        {
            return contentType;
        }
        
        return "application/octet-stream";
    }

    public void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            _logger.LogDebug("Created directory: {Path}", path);
        }
    }
}