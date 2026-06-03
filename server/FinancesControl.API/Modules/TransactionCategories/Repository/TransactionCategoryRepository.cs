using System.Collections.Generic;
using System.Linq;
using Dapper;
using FinancesControl.API.Common;
using FinancesControl.API.Data;
using FinancesControl.API.Models;
using FinancesControl.API.TransactionCategories.Repository.SqlCommands;

namespace FinancesControl.API.TransactionCategories.Repository;

public class TransactionCategoryRepository : ITransactionCategoryRepository
{
    private readonly AppDbConnectionFactory _connectionFactory;

    public TransactionCategoryRepository(AppDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<long> Add(TransactionCategory category)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new CreateTransactionCategoryCommand();
        var (sql, parameters) = command.Build(category);
        
        var id = await connection.QuerySingleOrDefaultAsync<long>(sql, parameters);
        return id;
    }

    public async Task<TransactionCategory?> GetById(long categoryId, string? userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var query = new GetTransactionCategoryByIdQuery();
        var (sql, parameters) = query.Build(categoryId, userId);
        
        var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, parameters);
        
        if (result == null)
            return null;

        return MapToTransactionCategory(result);
    }

    public async Task<List<TransactionCategory>> GetAll(string? userId, PaginationParams paginationParams)
    {
        using var connection = _connectionFactory.CreateConnection();
        var query = new GetAllTransactionCategoriesQuery();
        var (sql, parameters) = query.Build(userId, paginationParams);
        
        var results = (await connection.QueryAsync<dynamic>(sql, parameters)).ToList();
        
        return results.Select(MapToTransactionCategory).ToList();
    }

    public async Task<int> CountByUserId(string? userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var query = new CountTransactionCategoriesByUserQuery();
        var (sql, parameters) = query.Build(userId);

        return await connection.QuerySingleOrDefaultAsync<int>(sql, parameters);
    }

    public async Task<bool> Update(TransactionCategory category)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new UpdateTransactionCategoryCommand();
        var (sql, parameters) = command.Build(category);
        
        var rowsAffected = await connection.ExecuteAsync(sql, parameters);
        return rowsAffected > 0;
    }

    public async Task<bool> ExistsAsync(long categoryId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var query = new TransactionCategoryExistsQuery();
        var (sql, parameters) = query.Build(categoryId);
        
        return await connection.QuerySingleOrDefaultAsync<bool>(sql, parameters);
    }

    private static TransactionCategory MapToTransactionCategory(dynamic row)
    {
        return TransactionCategory.FromDatabase(
            id: (long)row.Id,
            name: row.Name,
            isActive: row.IsActive,
            isDefault: row.IsDefault,
            userId: row.UserId,
            createdAt: row.CreatedAt,
            updatedAt: row.UpdatedAt
        );
    }
}
