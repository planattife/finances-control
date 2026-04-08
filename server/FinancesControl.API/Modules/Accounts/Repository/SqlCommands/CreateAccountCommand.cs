using Dapper;

namespace FinancesControl.API.Accounts.Repository.SqlCommands;

public class CreateAccountCommand
{
    public (string Sql, DynamicParameters Params) Build(Account account)
    {
        var sql = @"
            INSERT INTO ""Accounts"" (""Name"", ""UserId"", ""IsActive"", ""CreatedAt"")
            VALUES (@Name, @UserId, @IsActive, @CreatedAt)
            RETURNING ""Id"";
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@Name", account.Name);
        parameters.Add("@UserId", account.UserId);
        parameters.Add("@IsActive", account.IsActive);
        parameters.Add("@CreatedAt", account.CreatedAt);

        return (sql, parameters);
    }
}
