namespace FinancesControl.API.Accounts.DTOs;

public record AccountListItemResponse
(
    long Id,
    string Name,
    bool IsActive
);