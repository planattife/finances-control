using System;
using System.Collections.Generic;
using FinancesControl.API.Transactions.DTOs;

namespace FinancesControl.API.Transactions.Services;

public interface ITransactionService
{
	long CreateTransaction(CreateTransactionRequest request);
	TransactionDetailResponse? GetById(long transactionId);
	List<TransactionListItemResponse> GetAll();
	bool Delete(long transactionId);
	bool UpdateAmount(long transactionId, UpdateTransactionAmountRequest request);
	bool UpdateCategory(long transactionId, UpdateTransactionCategoryRequest request);
}
