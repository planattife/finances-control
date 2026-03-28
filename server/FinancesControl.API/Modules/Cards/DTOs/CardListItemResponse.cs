using FinancesControl.API.Enums;

namespace FinancesControl.API.Cards.DTOs;

public record CardListItemResponse(
    int Id,
    string CardNumber,
    CardType CardType,
    CardNetwork CardNetwork,
    int BankId,
    string? CardName
);