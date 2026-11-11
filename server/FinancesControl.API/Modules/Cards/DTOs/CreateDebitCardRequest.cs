using FinancesControl.API.Enums;

namespace FinancesControl.API.Cards.DTOs;

public record CreateDebitCardRequest(
    string CardNumber,
    DateTime ExpirationDate,
    CardNetwork CardNetwork,
    long BankId,
    string? CardName = null
);