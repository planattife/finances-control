using System.ComponentModel.DataAnnotations;
using FinancesControl.API.Enums;

namespace FinancesControl.API.Banks.DTOs;

public record CreateBankRequest(
    [Required]
    [MinLength(2)]
    [MaxLength(50)]
    string Name,
    [Required]
    BankAccountType AccountType
);