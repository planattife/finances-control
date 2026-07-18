using FinancesControl.API.Common;
using FluentAssertions;

namespace FinancesControl.Tests.Common;

public class PaginationParamsTests
{
    [Fact]
    public void Constructor_WithDefaultValues_UsesDefaultPagination()
    {
        var paginationParams = new PaginationParams();

        paginationParams.PageSize.Should().Be(10);
        paginationParams.PageNumber.Should().Be(1);
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(0, 10)]
    [InlineData(1, 1)]
    [InlineData(50, 50)]
    [InlineData(PaginationParams.MaxPageSize, PaginationParams.MaxPageSize)]
    [InlineData(PaginationParams.MaxPageSize + 1, PaginationParams.MaxPageSize)]
    public void PageSize_WithValue_NormalizesExpectedPageSize(int pageSize, int expectedPageSize)
    {
        var paginationParams = new PaginationParams { PageSize = pageSize };

        paginationParams.PageSize.Should().Be(expectedPageSize);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void PageNumber_WithInvalidValue_UsesDefaultPageNumber(int pageNumber)
    {
        var paginationParams = new PaginationParams { PageNumber = pageNumber };

        paginationParams.PageNumber.Should().Be(1);
    }

    [Fact]
    public void GetOffset_WithOversizedPageSize_UsesClampedPageSize()
    {
        var paginationParams = new PaginationParams
        {
            PageNumber = 2,
            PageSize = PaginationParams.MaxPageSize + 1
        };

        paginationParams.GetOffset().Should().Be(PaginationParams.MaxPageSize);
    }
}
