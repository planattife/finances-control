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

    public async Task<long> Add(Account account)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new CreateAccountCommand();
        var (sql, parameters) = command.Build(account);
        
        var id = await connection.QuerySingleOrDefaultAsync<long>(sql, parameters);
        return id;
    }

    public async Task<Account?> GetById(long accountId, string userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var query = new GetAccountByIdQuery();
        var (sql, parameters) = query.Build(accountId, userId);
        
        var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, parameters);
        
        if (result == null)
            return null;

        return MapToAccount(result);
    }

    public async Task<List<Account>> GetAll(string userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var query = new GetAllAccountsQuery();
        var (sql, parameters) = query.Build(userId);
        
        var results = (await connection.QueryAsync<dynamic>(sql, parameters)).ToList();
        
        return results.Select(MapToAccount).ToList();
    }

    public async Task<bool> Update(Account account)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new UpdateAccountCommand();
        var (sql, parameters) = command.Build(account);
        
        var rowsAffected = await connection.ExecuteAsync(sql, parameters);
        return rowsAffected > 0;
    }

    public async Task<bool> ExistsAsync(long accountId, string userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var query = new AccountExistsQuery();
        var (sql, parameters) = query.Build(accountId, userId);
        
        return await connection.QuerySingleAsync<bool>(sql, parameters);
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

