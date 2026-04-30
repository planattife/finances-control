namespace FinancesControl.API.Cards.Repository.SqlCommands;

public class GetAllCardsQuery
{
    public string Sql => @"
        SELECT ""Id"", ""CardType"", ""CardNumber"", ""ExpirationDate"", ""IsActive"", 
               ""CardNetwork"", ""BankId"", ""CardName"", ""CreatedAt"", ""UpdatedAt""
        FROM ""Cards""
        ORDER BY ""CreatedAt"" DESC;
    ";
}
