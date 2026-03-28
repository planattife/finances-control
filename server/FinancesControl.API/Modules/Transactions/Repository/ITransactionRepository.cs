using System.Collections.Generic;
using FinancesControl.API.Models;

namespace FinancesControl.API.Transactions.Repository;

public interface ITransactionRepository
{
	int Add(Transaction transaction);
	Transaction? GetById(int transactionId);
	List<Transaction> GetAll();
	bool Update(Transaction transaction);
}
