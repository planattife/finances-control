using FinancesControl.API.Models;
using FinancesControl.API.Common;

namespace FinancesControl.API.Banks.Repository;

public interface IBankRepository
{
    Task<long> Add(Bank bank);
    Task<Bank?> GetById(long bankId, string userId);
    Task<List<Bank>> GetAll(string userId, PaginationParams paginationParams);
    Task<int> CountByUserId(string userId);
    Task<bool> Update(Bank bank);
    Task<bool> ExistsAsync(long bankId, string userId);
}
