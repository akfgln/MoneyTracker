using MoneyTracker.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Application.DTOs.Transaction;

public class CreateTransactionDto
{
    public Guid AccountId { get; set; }
    public string Currency { get; set; } = "EUR";
    public DateTime TransactionDate { get; set; }
    public DateTime? BookingDate { get; set; }
    public string? MerchantName { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal? CustomVatRate { get; set; } // Override default category VAT rate
    public string? ReferenceNumber { get; set; }
    public List<string>? Tags { get; set; }
    public bool IsRecurring { get; set; } = false;
    public string? RecurrencePattern { get; set; }

    [Required(ErrorMessage = "Datum ist erforderlich")]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "Betrag ist erforderlich")]
    [Range(0.01, 999999.99, ErrorMessage = "Betrag muss zwischen 0,01 € und 999.999,99 € liegen")]
    public decimal Amount { get; set; }

    [StringLength(500, ErrorMessage = "Beschreibung muss max 500 Zeichen lang sein")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kategorie ist erforderlich")]
    public Guid CategoryId { get; set; }

    [Required(ErrorMessage = "Transaktionstyp ist erforderlich")]
    public TransactionType Type { get; set; }

    [Range(0, 1, ErrorMessage = "MwSt.-Satz muss zwischen 0% und 100% liegen")]
    public decimal? VatRate { get; set; }

    [StringLength(100, ErrorMessage = "Rechnungsnummer darf maximal 100 Zeichen lang sein")]
    public string? InvoiceNumber { get; set; }

    [StringLength(200, ErrorMessage = "Lieferantenname darf maximal 200 Zeichen lang sein")]
    public string? Supplier { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    [StringLength(1000, ErrorMessage = "Notizen dürfen maximal 1000 Zeichen lang sein")]
    public string? Notes { get; set; }

    [StringLength(200, ErrorMessage = "Standort darf maximal 200 Zeichen lang sein")]
    public string? Location { get; set; }
}