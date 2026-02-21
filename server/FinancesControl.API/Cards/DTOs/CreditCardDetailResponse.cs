using FinancesControl.API.Enums;

namespace FinancesControl.API.Cards.DTOs;

public record CreditCardDetailResponse(
    int Id,
    string CardNumber,
    CardType CardType,
    CardNetwork CardNetwork,
    DateTime ExpirationDate,
    int BankId,
    string CardName,
    DateTime NextClosingDay,
    DateTime NextDueDateDay,
    string Status,
    DateTime CreatedAt,
    DateTime? LastUpdate
);