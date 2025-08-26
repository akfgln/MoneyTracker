using MoneyTracker.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Application.DTOs.Transaction;

public class UpdateTransactionDto : CreateTransactionDto
{
    [Required(ErrorMessage = "ID ist erforderlich")]
    public Guid Id { get; set; }
}