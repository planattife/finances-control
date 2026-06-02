using System.Collections.Generic;
using System.Linq;
using Dapper;
using FinancesControl.API.Common;
using FinancesControl.API.Data;
using FinancesControl.API.Enums;
using FinancesControl.API.Models;
using FinancesControl.API.Transactions.Repository.SqlCommands;

namespace FinancesControl.API.Transactions.Repository;

public class TransactionRepository : ITransactionRepository
{
	private readonly AppDbConnectionFactory _connectionFactory;

	public TransactionRepository(AppDbConnectionFactory connectionFactory)
	{
		_connectionFactory = connectionFactory;
	}

	public async Task<long> Add(Transaction transaction)
	{
		using var connection = _connectionFactory.CreateConnection();
		var command = new CreateTransactionCommand();
		var (sql, parameters) = command.Build(transaction);
		
		var id = await connection.QuerySingleOrDefaultAsync<long>(sql, parameters);
		return id;
	}

	public async Task<Transaction?> GetById(long transactionId, long accountId)
	{
		using var connection = _connectionFactory.CreateConnection();
		var query = new GetTransactionByIdQuery();
		var (sql, parameters) = query.Build(transactionId, accountId);
		
		var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, parameters);
		
		if (result == null)
			return null;

		return MapToTransaction(result);
	}

	public async Task<List<Transaction>> GetAll(long accountId, PaginationParams paginationParams)
	{
		using var connection = _connectionFactory.CreateConnection();
		var query = new GetAllTransactionsQuery();
		var (sql, parameters) = query.Build(accountId, paginationParams);
		
		var results = (await connection.QueryAsync<dynamic>(sql, parameters)).ToList();
		var transactions = new List<Transaction>();
		
		foreach (var result in results)
		{
			transactions.Add(MapToTransaction(result));
		}
		
		return transactions;
	}

	public async Task<int> CountByAccountId(long accountId)
	{
		using var connection = _connectionFactory.CreateConnection();
		var query = new CountTransactionsByAccountQuery();
		var (sql, parameters) = query.Build(accountId);

		return await connection.QuerySingleOrDefaultAsync<int>(sql, parameters);
	}

	public async Task<bool> Update(Transaction transaction)
	{
		using var connection = _connectionFactory.CreateConnection();
		var command = new UpdateTransactionCommand();
		var (sql, parameters) = command.Build(transaction);
		
		var rowsAffected = await connection.ExecuteAsync(sql, parameters);
		return rowsAffected > 0;
	}

	public async Task<bool> ExistsAsync(long transactionId, long accountId)
	{
		using var connection = _connectionFactory.CreateConnection();
		var query = new TransactionExistsQuery();
		var (sql, parameters) = query.Build(transactionId, accountId);
		
		return await connection.QuerySingleOrDefaultAsync<bool>(sql, parameters);
	}

	private static Transaction MapToTransaction(dynamic row)
	{
		var id = (long)row.Id;
		var amount = (decimal)row.Amount;
		var paymentMethod = (PaymentMethod)row.PaymentMethod;
		var transactionType = (TransactionType)row.TransactionType;
		var transactionDate = (DateTime)row.TransactionDate;
		var categoryId = (long)row.CategoryId;
		var accountId = (long)row.AccountId;
		var isActive = (bool)row.IsActive;
		var cardId = row.CardId;
		var bankId = row.BankId;
		var createdAt = (DateTime)row.CreatedAt;
		var updatedAt = (DateTime?)row.UpdatedAt;
		var description = (string?)row.Description;

		// Determine the transaction type based on CardId and BankId
		if (cardId != null && cardId > 0)
		{
			return CardTransaction.FromDatabase(id, amount, paymentMethod, transactionType,
				transactionDate, categoryId, accountId, (long)cardId, isActive, createdAt, updatedAt, description);
		}

		if (bankId != null && bankId > 0)
		{
			return GeneralTransaction.FromDatabase(id, amount, paymentMethod, transactionType,
				transactionDate, categoryId, accountId, (long)bankId, isActive, createdAt, updatedAt, description);
		}

		// Default to basic Transaction (for Cash payments)
		return Transaction.FromDatabase(id, amount, paymentMethod, transactionType,
			transactionDate, categoryId, accountId, isActive, createdAt, updatedAt, description);
	}
}
