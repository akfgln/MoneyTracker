using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Application.DTOs.File;
using Microsoft.Extensions.Logging;

namespace MoneyTracker.Infrastructure.Services;

public class PdfTextExtractionService : IPdfTextExtractionService
{
    private readonly ILogger<PdfTextExtractionService> _logger;

    public PdfTextExtractionService(ILogger<PdfTextExtractionService> logger)
    {
        _logger = logger;
    }

    public async Task<string> ExtractTextAsync(byte[] pdfBytes)
    {
        try
        {
            using var document = PdfDocument.Open(pdfBytes);
            var textBuilder = new StringBuilder();

            foreach (var page in document.GetPages())
            {
                var pageText = page.Text;
                textBuilder.AppendLine(pageText);
                textBuilder.AppendLine(); // Add line break between pages
            }

            var extractedText = textBuilder.ToString();
            _logger.LogInformation("Successfully extracted {CharacterCount} characters from PDF with {PageCount} pages", 
                extractedText.Length, document.NumberOfPages);

            return extractedText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract text from PDF");
            throw new InvalidOperationException("PDF-Text konnte nicht extrahiert werden.", ex);
        }
    }

    public async Task<bool> ValidatePdfAsync(byte[] pdfBytes)
    {
        try
        {
            using var document = PdfDocument.Open(pdfBytes);
            
            // Basic validation checks
            if (document.NumberOfPages == 0)
            {
                _logger.LogWarning("PDF has no pages");
                return false;
            }

            // Check if we can read at least one page
            var firstPage = document.GetPage(1);
            if (firstPage == null)
            {
                _logger.LogWarning("Cannot read first page of PDF");
                return false;
            }

            // Try to extract some text to ensure it's not just images
            var text = firstPage.Text;
            if (string.IsNullOrWhiteSpace(text) && document.NumberOfPages == 1)
            {
                _logger.LogWarning("PDF appears to contain no text (might be image-only)");
                // Don't fail validation for image-only PDFs, but log it
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF validation failed");
            return false;
        }
    }

    public async Task<PdfMetadata> GetPdfMetadataAsync(byte[] pdfBytes)
    {
        try
        {
            using var document = PdfDocument.Open(pdfBytes);
            var info = document.Information;

            return new PdfMetadata
            {
                NumberOfPages = document.NumberOfPages,
                Title = info.Title,
                Author = info.Author,
                CreationDate = DateTime.Parse(info.CreationDate),
                Creator = info.Creator,
                Subject = info.Subject,
                Keywords = info.Keywords,
                FileSize = pdfBytes.Length,
                PdfVersion = document.Version.ToString(),
                IsEncrypted = document.IsEncrypted,
                HasDigitalSignature = HasDigitalSignatures(document)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract PDF metadata");
            throw new InvalidOperationException("PDF-Metadaten konnten nicht extrahiert werden.", ex);
        }
    }

    public async Task<bool> IsPdfEncryptedAsync(byte[] pdfBytes)
    {
        try
        {
            using var document = PdfDocument.Open(pdfBytes);
            return document.IsEncrypted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check PDF encryption status");
            return false;
        }
    }

    public async Task<int> GetPageCountAsync(byte[] pdfBytes)
    {
        try
        {
            using var document = PdfDocument.Open(pdfBytes);
            return document.NumberOfPages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get PDF page count");
            throw new InvalidOperationException("PDF-Seitenzahl konnte nicht ermittelt werden.", ex);
        }
    }

    private bool HasDigitalSignatures(PdfDocument document)
    {
        try
        {
            // Check for signature fields or digital signatures
            // This is a simplified check - in a production environment,
            // you might want to use a more sophisticated signature detection
            foreach (var page in document.GetPages())
            {
                if (page.ExperimentalAccess.GetAnnotations().Any(annotation => 
                    annotation.Type.ToString().Contains("Sig")))
                {
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not check for digital signatures");
            return false;
        }
    }
}