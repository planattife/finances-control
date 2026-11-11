using System;
using System.Collections.Generic;
using FinancesControl.API.Transactions.DTOs;
using FinancesControl.API.Common;

namespace FinancesControl.API.Transactions.Services;

public interface ITransactionService
{
	Task<long> CreateTransaction(CreateTransactionRequest request, long accountId, string? userId);
	Task<SingleResponse<TransactionDetailResponse>> GetById(long transactionId, long accountId, string? userId);
	Task<PaginatedResponse<TransactionListItemResponse>> GetAll(long accountId, string? userId, PaginationParams paginationParams);
	Task<bool> Delete(long transactionId, long accountId, string? userId);
	Task<bool> Update(long transactionId, long accountId, UpdateTransactionRequest request, string? userId);
	Task<bool> ExistsAsync(long transactionId, long accountId);
}
