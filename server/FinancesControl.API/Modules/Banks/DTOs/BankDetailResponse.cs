using FinancesControl.API.Enums;

namespace FinancesControl.API.Banks.DTOs;

public record BankDetailResponse(
    int Id,
    string Name,
    BankAccountType AccountType,
    string Status,
    DateTime CreatedAt,
    DateTime? LastUpdate
);