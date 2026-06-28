using FinancesControl.API.Enums;

namespace FinancesControl.API.Cards.DTOs;

public record CreditCardDetailResponse(
    long Id,
    string CardNumber,
    CardType CardType,
    CardNetwork CardNetwork,
    DateTime ExpirationDate,
    CardBankResponse Bank,
    string CardName,
    DateTime NextClosingDay,
    DateTime NextDueDateDay,
    string Status,
    DateTime CreatedAt,
    DateTime? LastUpdate
);
