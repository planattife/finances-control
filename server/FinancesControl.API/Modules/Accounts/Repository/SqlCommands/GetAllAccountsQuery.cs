namespace FinancesControl.API.Accounts.Repository.SqlCommands;

public class GetAllAccountsQuery
{
    public string Sql => @"
        SELECT ""Id"", ""Name"", ""UserId"", ""IsActive"", ""CreatedAt"", ""UpdatedAt""
        FROM ""Accounts""
        ORDER BY ""CreatedAt"" DESC;
    ";
}
