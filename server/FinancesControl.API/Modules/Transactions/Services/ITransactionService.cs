using System;
using System.Collections.Generic;
using FinancesControl.API.Transactions.DTOs;

namespace FinancesControl.API.Transactions.Services;

public interface ITransactionService
{
	int CreateTransaction(CreateTransactionRequest request);
	TransactionDetailResponse? GetById(int transactionId);
	List<TransactionListItemResponse> GetAll();
	bool Delete(int transactionId);
	bool UpdateAmount(int transactionId, UpdateTransactionAmountRequest request);
	bool UpdateCategory(int transactionId, UpdateTransactionCategoryRequest request);
}
