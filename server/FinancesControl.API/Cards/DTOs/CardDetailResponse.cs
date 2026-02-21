using FinancesControl.API.Enums;

namespace FinancesControl.API.Cards.DTOs;

public record CardDetailResponse(
    int Id,
    string CardNumber,
    CardType CardType,
    CardNetwork CardNetwork,
    DateTime ExpirationDate,
    int BankId,
    string CardName,
    string Status,
    DateTime CreatedAt,
    DateTime? LastUpdate
);