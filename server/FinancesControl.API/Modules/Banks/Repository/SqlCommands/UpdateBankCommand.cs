using Dapper;

namespace FinancesControl.API.Banks.Repository.SqlCommands;

public class UpdateBankCommand
{
    public (string Sql, DynamicParameters Params) Build(Bank bank)
    {
        var sql = @"
            UPDATE ""Banks""
            SET ""Name"" = @Name, ""AccountType"" = @AccountType, ""IsActive"" = @IsActive, ""UpdatedAt"" = @UpdatedAt
            WHERE ""Id"" = @Id;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@Id", bank.Id);
        parameters.Add("@Name", bank.Name);
        parameters.Add("@AccountType", (int)bank.AccountType);
        parameters.Add("@IsActive", bank.IsActive);
        parameters.Add("@UpdatedAt", bank.UpdatedAt ??DateTime.UtcNow);

        return (sql, parameters);
    }
}
