namespace FinancesControl.API.Common.RateLimiting;

public sealed class ApiRateLimitOptions
{
    public int AuthPermitLimit { get; init; } = 5;
    public TimeSpan AuthWindow { get; init; } = TimeSpan.FromMinutes(1);
    public int AuthQueueLimit { get; init; } = 0;

    public int FinanceWritePermitLimit { get; init; } = 60;
    public TimeSpan FinanceWriteWindow { get; init; } = TimeSpan.FromMinutes(1);
    public int FinanceWriteQueueLimit { get; init; } = 0;
}
