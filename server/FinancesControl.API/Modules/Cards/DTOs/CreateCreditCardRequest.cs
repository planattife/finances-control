using FinancesControl.API.Enums;

namespace FinancesControl.API.Cards.DTOs;

public record CreateCreditCardRequest(
    string CardNumber,
    DateTime ExpirationDate,
    CardNetwork CardNetwork,
    int BankId,
    int ClosingDay,
    int DueDateDay,
    string? CardName = null
);