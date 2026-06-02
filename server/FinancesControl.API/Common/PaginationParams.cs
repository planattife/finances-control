namespace FinancesControl.API.Common;

public class PaginationParams
{
    private const int DefaultPageSize = 10;
    private const int DefaultPageNumber = 1;

    private int _pageSize = DefaultPageSize;
    private int _pageNumber = DefaultPageNumber;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? DefaultPageSize : value;
    }

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? DefaultPageNumber : value;
    }

    private int Offset => (PageNumber - 1) * PageSize;

    public int GetOffset() => Offset;
}
