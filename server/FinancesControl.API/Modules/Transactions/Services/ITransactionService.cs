using System;
using System.Collections.Generic;
using FinancesControl.API.Transactions.DTOs;

namespace FinancesControl.API.Transactions.Services;

public interface ITransactionService
{
	Task<long> CreateTransaction(CreateTransactionRequest request, long accountId, string? userId);
	Task<TransactionDetailResponse?> GetById(long transactionId, long accountId, string? userId);
	Task<List<TransactionListItemResponse>> GetAll(long accountId, string? userId);
	Task<bool> Delete(long transactionId, long accountId, string? userId);
	Task<bool> Update(long transactionId, long accountId, UpdateTransactionRequest request, string? userId);
}
