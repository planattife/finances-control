using System.Collections.Generic;
using FinancesControl.API.Models;

namespace FinancesControl.API.Transactions.Repository;

public interface ITransactionRepository
{
	long Add(Transaction transaction);
	Transaction? GetById(long transactionId);
	List<Transaction> GetAll();
	bool Update(Transaction transaction);
}
