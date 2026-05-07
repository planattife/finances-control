using System;
using System.Collections.Generic;
using FinancesControl.API.Transactions.DTOs;

namespace FinancesControl.API.Transactions.Services;

public interface ITransactionService
{
	long CreateTransaction(CreateTransactionRequest request, string? userId);
	TransactionDetailResponse? GetById(long transactionId);
	List<TransactionListItemResponse> GetAll();
	bool Delete(long transactionId);
	bool Update(long transactionId, UpdateTransactionRequest request, string? userId);
}
