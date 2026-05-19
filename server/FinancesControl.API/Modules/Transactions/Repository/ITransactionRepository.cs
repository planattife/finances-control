using System.Collections.Generic;
using FinancesControl.API.Models;

namespace FinancesControl.API.Transactions.Repository;

public interface ITransactionRepository
{
	Task<long> Add(Transaction transaction);
	Task<Transaction?> GetById(long transactionId, long accountId);
	Task<List<Transaction>> GetAll(long accountId);
	Task<bool> Update(Transaction transaction);
}
