namespace FinancesControl.API.Accounts.DTOs;

public record AccountKpiResponse
(
    long AccountId,
    int CardsCount,
    int TransactionsCount
);
