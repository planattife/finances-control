namespace FinancesControl.API.Common;

public class PaginationParams
{
    /// <summary>
    /// Maximum number of records allowed per page for list endpoints.
    /// </summary>
    public const int MaxPageSize = 100;

    private const int DefaultPageSize = 10;
    private const int DefaultPageNumber = 1;

    private int _pageSize = DefaultPageSize;
    private int _pageNumber = DefaultPageNumber;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? DefaultPageSize : Math.Min(value, MaxPageSize);
    }

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? DefaultPageNumber : value;
    }

    private int Offset => (PageNumber - 1) * PageSize;

    public int GetOffset() => Offset;
}
