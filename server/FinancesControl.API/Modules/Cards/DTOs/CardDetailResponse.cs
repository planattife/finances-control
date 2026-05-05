using FinancesControl.API.Enums;

namespace FinancesControl.API.Cards.DTOs;

public record CardDetailResponse(
    long Id,
    string CardNumber,
    CardType CardType,
    CardNetwork CardNetwork,
    DateTime ExpirationDate,
    long BankId,
    string CardName,
    string Status,
    DateTime CreatedAt,
    DateTime? LastUpdate
);