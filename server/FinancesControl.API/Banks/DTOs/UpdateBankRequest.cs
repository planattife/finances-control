using FinancesControl.API.Enums;

namespace FinancesControl.API.Banks.DTOs;

public record UpdateBankRequest(
    string Name,
    BankAccountType AccountType
);