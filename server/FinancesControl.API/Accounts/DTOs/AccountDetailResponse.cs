namespace FinancesControl.API.Accounts.DTOs;

public record AccountDetailResponse
(
    int Id,
    string Name,
    string Status,
    DateTime CreatedAt,
    DateTime? LastUpdate
);
