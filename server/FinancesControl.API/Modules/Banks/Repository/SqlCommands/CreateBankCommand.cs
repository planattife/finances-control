using Dapper;

namespace FinancesControl.API.Banks.Repository.SqlCommands;

public class CreateBankCommand
{
    public (string Sql, DynamicParameters Params) Build(Bank bank)
    {
        var sql = @"
            INSERT INTO ""Banks"" (""Name"", ""AccountType"", ""IsActive"", ""UserId"", ""CreatedAt"")
            VALUES (@Name, @AccountType, @IsActive, @UserId, @CreatedAt)
            RETURNING ""Id"";
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@Name", bank.Name);
        parameters.Add("@AccountType", (int)bank.AccountType);
        parameters.Add("@IsActive", bank.IsActive);
        parameters.Add("@UserId", bank.UserId);
        parameters.Add("@CreatedAt", bank.CreatedAt);

        return (sql, parameters);
    }
}
