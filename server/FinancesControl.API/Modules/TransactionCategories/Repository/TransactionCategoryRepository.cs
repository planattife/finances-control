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

    public int Add(TransactionCategory category)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new CreateTransactionCategoryCommand();
        var (sql, parameters) = command.Build(category);
        
        var id = connection.QuerySingleOrDefault<long>(sql, parameters);
        return (int)id;
    }

    public TransactionCategory? GetById(int categoryId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var query = new GetTransactionCategoryByIdQuery();
        var (sql, parameters) = query.Build(categoryId);
        
        var result = connection.QuerySingleOrDefault<dynamic>(sql, parameters);
        
        if (result == null)
            return null;

        return MapToTransactionCategory(result);
    }

    public List<TransactionCategory> GetAll()
    {
        using var connection = _connectionFactory.CreateConnection();
        var query = new GetAllTransactionCategoriesQuery();
        
        var results = connection.Query<dynamic>(query.Sql).ToList();
        
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
            id: (int)row.Id,
            name: row.Name,
            isActive: row.IsActive,
            createdAt: row.CreatedAt,
            updatedAt: row.UpdatedAt
        );
    }
}
