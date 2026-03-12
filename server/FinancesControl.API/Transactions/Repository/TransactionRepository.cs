using System.Collections.Generic;
using System.Linq;
using FinancesControl.API.Models;

namespace FinancesControl.API.Transactions.Repository;

public class TransactionRepository : ITransactionRepository
{
	private static readonly List<Transaction> _transactions = new();

	public int Add(Transaction transaction)
	{
		_transactions.Add(transaction);
		return transaction.Id;
	}

	public Transaction? GetById(int transactionId)
	{
		return _transactions.FirstOrDefault(t => t.Id == transactionId);
	}

	public List<Transaction> GetAll()
	{
		return _transactions.ToList();
	}

	public bool Update(Transaction transaction)
	{
		var idx = _transactions.FindIndex(t => t.Id == transaction.Id);
		if (idx == -1) return false;
		_transactions[idx] = transaction;
		return true;
	}
}
