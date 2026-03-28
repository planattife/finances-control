using FinancesControl.API.Enums;

namespace FinancesControl.API.Cards.DTOs;

public record UpdateCreditCardRequest(
    int ClosingDay,
    int DueDateDay
);