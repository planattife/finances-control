using FinancesControl.API.Enums;

namespace FinancesControl.API.Banks.DTOs;

public record BankListItemResponse(
    long Id,
    string Name,
    BankAccountType AccountType
);