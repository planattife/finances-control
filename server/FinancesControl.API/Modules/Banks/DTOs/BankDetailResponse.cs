using FinancesControl.API.Enums;

namespace FinancesControl.API.Banks.DTOs;

public record BankDetailResponse(
    long Id,
    string Name,
    BankAccountType AccountType,
    string Status,
    DateTime CreatedAt,
    DateTime? LastUpdate
);