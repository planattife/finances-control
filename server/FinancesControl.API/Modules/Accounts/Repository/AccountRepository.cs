using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using FinancesControl.API.Accounts.Repository.SqlCommands;
using FinancesControl.API.Data;

namespace FinancesControl.API.Accounts.Repository;

public class AccountRepository : IAccountRepository
{
    private readonly AppDbConnectionFactory _connectionFactory;

    public AccountRepository(AppDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public long Add(Account account)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new CreateAccountCommand();
        var (sql, parameters) = command.Build(account);
        
        var id = connection.QuerySingleOrDefault<long>(sql, parameters);
        return id;
    }

    public Account? GetById(long accountId, string userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var query = new GetAccountByIdQuery();
        var (sql, parameters) = query.Build(accountId, userId);
        
        var result = connection.QuerySingleOrDefault<dynamic>(sql, parameters);
        
        if (result == null)
            return null;

        return MapToAccount(result);
    }

    public List<Account> GetAll(string userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var query = new GetAllAccountsQuery();
        var (sql, parameters) = query.Build(userId);
        
        var results = connection.Query<dynamic>(sql, parameters).ToList();
        
        return results.Select(MapToAccount).ToList();
    }

    public bool Update(Account account)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new UpdateAccountCommand();
        var (sql, parameters) = command.Build(account);
        
        var rowsAffected = connection.Execute(sql, parameters);
        return rowsAffected > 0;
    }

    private static Account MapToAccount(dynamic row)
    {
        return Account.FromDatabase(
            id: (long)row.Id,
            name: row.Name,
            userId: row.UserId,
            isActive: row.IsActive,
            createdAt: row.CreatedAt,
            updatedAt: row.UpdatedAt
        );
    }
}

