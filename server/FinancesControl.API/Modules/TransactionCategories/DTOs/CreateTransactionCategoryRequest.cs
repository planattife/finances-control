using System.ComponentModel.DataAnnotations;

namespace FinancesControl.API.TransactionCategories.DTOs;

public record CreateTransactionCategoryRequest(
    [Required]
    [MinLength(2)]
    [MaxLength(50)]
    string Name
);