using Dapper;

namespace FinancesControl.API.Accounts.Repository.SqlCommands;

public class UpdateAccountCommand
{
    public (string Sql, DynamicParameters Params) Build(Account account)
    {
        var sql = @"
            UPDATE ""Accounts""
            SET ""Name"" = @Name, ""IsActive"" = @IsActive, ""UpdatedAt"" = @UpdatedAt
            WHERE ""Id"" = @Id AND ""UserId"" = @UserId;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@Id", account.Id);
        parameters.Add("@Name", account.Name);
        parameters.Add("@IsActive", account.IsActive);
        parameters.Add("@UpdatedAt", account.UpdatedAt ?? DateTime.UtcNow);
        parameters.Add("@UserId", account.UserId);

        return (sql, parameters);
    }
}
