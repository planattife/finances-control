using Dapper;
using FinancesControl.API.Common;
using FinancesControl.API.Transactions.Repository.SqlCommands;
using FluentAssertions;

namespace FinancesControl.Tests.Transactions.Repository.SqlCommands;

public class TransactionSqlCommandTests
{
    [Fact]
    public void GetAllTransactionsQuery_Build_FiltersActiveTransactions()
    {
        var query = new GetAllTransactionsQuery();
        var paginationParams = new PaginationParams { PageNumber = 2, PageSize = 5 };

        var (sql, parameters) = query.Build(10, paginationParams);

        sql.Should().Contain(@"t.""AccountId"" = @AccountId");
        sql.Should().Contain(@"t.""IsActive"" = TRUE");
        parameters.Get<long>("@AccountId").Should().Be(10);
        parameters.Get<int>("@PageSize").Should().Be(5);
        parameters.Get<int>("@Offset").Should().Be(5);
    }

    [Fact]
    public void CountTransactionsByAccountQuery_Build_FiltersActiveTransactions()
    {
        var query = new CountTransactionsByAccountQuery();

        var (sql, parameters) = query.Build(10);

        sql.Should().Contain(@"""AccountId"" = @AccountId");
        sql.Should().Contain(@"""IsActive"" = TRUE");
        parameters.Get<long>("@AccountId").Should().Be(10);
    }
}
