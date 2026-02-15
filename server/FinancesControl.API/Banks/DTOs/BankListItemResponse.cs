using FinancesControl.API.Enums;

namespace FinancesControl.API.Banks.DTOs;

public record BankListItemResponse(
    int Id,
    string Name,
    BankAccountType AccountType
);