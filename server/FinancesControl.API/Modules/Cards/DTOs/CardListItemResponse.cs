using FinancesControl.API.Enums;

namespace FinancesControl.API.Cards.DTOs;

public record CardListItemResponse(
    long Id,
    string CardNumber,
    CardType CardType,
    CardNetwork CardNetwork,
    long BankId,
    string? CardName
);