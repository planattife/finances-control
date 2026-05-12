using System.Collections.Generic;
using System.Linq;
using Dapper;
using FinancesControl.API.Banks.Repository.SqlCommands;
using FinancesControl.API.Data;
using FinancesControl.API.Enums;
using FinancesControl.API.Models;

namespace FinancesControl.API.Banks.Repository;

public class BankRepository : IBankRepository
{
    private readonly AppDbConnectionFactory _connectionFactory;

    public BankRepository(AppDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public long Add(Bank bank)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new CreateBankCommand();
        var (sql, parameters) = command.Build(bank);
        
        var id = connection.QuerySingleOrDefault<long>(sql, parameters);
        return id;
    }

    public Bank? GetById(long bankId, string userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var query = new GetBankByIdQuery();
        var (sql, parameters) = query.Build(bankId, userId);
        
        var result = connection.QuerySingleOrDefault<dynamic>(sql, parameters);
        
        if (result == null)
            return null;

        return MapToBank(result);
    }

    public List<Bank> GetAll(string userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var query = new GetAllBanksQuery();
        var (sql, parameters) = query.Build(userId);
        
        var results = connection.Query<dynamic>(sql, parameters).ToList();
        
        return results.Select(MapToBank).ToList();
    }

    public bool Update(Bank bank)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new UpdateBankCommand();
        var (sql, parameters) = command.Build(bank);
        
        var rowsAffected = connection.Execute(sql, parameters);
        return rowsAffected > 0;
    }

    private static Bank MapToBank(dynamic row)
    {
        return Bank.FromDatabase(
            id: (long)row.Id,
            name: row.Name,
            accountType: (BankAccountType)row.AccountType,
            userId: row.UserId,
            isActive: row.IsActive,
            createdAt: row.CreatedAt,
            updatedAt: row.UpdatedAt
        );
    }
}