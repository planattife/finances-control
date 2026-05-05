namespace FinancesControl.API.Accounts.DTOs;

public record AccountDetailResponse
(
    long Id,
    string Name,
    string Status,
    DateTime CreatedAt,
    DateTime? LastUpdate
);
