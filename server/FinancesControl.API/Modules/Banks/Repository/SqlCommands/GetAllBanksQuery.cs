namespace FinancesControl.API.Banks.Repository.SqlCommands;

public class GetAllBanksQuery
{
    public string Sql => @"
        SELECT ""Id"", ""Name"", ""AccountType"", ""IsActive"", ""UserId"", ""CreatedAt"", ""UpdatedAt""
        FROM ""Banks""
        ORDER BY ""CreatedAt"" DESC;
    ";
}
