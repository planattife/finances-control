using System;
using System.Collections.Generic;
using FinancesControl.API.Transactions.DTOs;

namespace FinancesControl.API.Transactions.Services;

public interface ITransactionService
{
	long CreateTransaction(CreateTransactionRequest request, long accountId, string? userId);
	TransactionDetailResponse? GetById(long transactionId, long accountId, string? userId);
	List<TransactionListItemResponse> GetAll(long accountId, string? userId);
	bool Delete(long transactionId, long accountId, string? userId);
	bool Update(long transactionId, long accountId, UpdateTransactionRequest request, string? userId);
}
