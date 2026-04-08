using Dapper;

namespace FinancesControl.API.Banks.Repository.SqlCommands;

public class GetBankByIdQuery
{
    public (string Sql, DynamicParameters Params) Build(long bankId)
    {
        var sql = @"
            SELECT ""Id"", ""Name"", ""AccountType"", ""IsActive"", ""UserId"", ""CreatedAt"", ""UpdatedAt""
            FROM ""Banks""
            WHERE ""Id"" = @Id;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@Id", bankId);

        return (sql, parameters);
    }
}
