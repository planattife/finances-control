using System.ComponentModel.DataAnnotations;

namespace FinancesControl.API.Accounts.DTOs;

public record CreateAccountRequest(
    [Required]
    [MinLength(2)]
    [MaxLength(50)]
    string Name
);