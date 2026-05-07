using System.Collections.Generic;
using System.Linq;
using Dapper;
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

    public long Add(TransactionCategory category)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new CreateTransactionCategoryCommand();
        var (sql, parameters) = command.Build(category);
        
        var id = connection.QuerySingleOrDefault<long>(sql, parameters);
        return id;
    }

    public TransactionCategory? GetById(long categoryId, string? userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var query = new GetTransactionCategoryByIdQuery();
        var (sql, parameters) = query.Build(categoryId, userId);
        
        var result = connection.QuerySingleOrDefault<dynamic>(sql, parameters);
        
        if (result == null)
            return null;

        return MapToTransactionCategory(result);
    }

    public List<TransactionCategory> GetAll(string? userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var query = new GetAllTransactionCategoriesQuery();
        var (sql, parameters) = query.Build(userId);
        
        var results = connection.Query<dynamic>(sql, parameters).ToList();
        
        return results.Select(MapToTransactionCategory).ToList();
    }

    public bool Update(TransactionCategory category)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new UpdateTransactionCategoryCommand();
        var (sql, parameters) = command.Build(category);
        
        var rowsAffected = connection.Execute(sql, parameters);
        return rowsAffected > 0;
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
