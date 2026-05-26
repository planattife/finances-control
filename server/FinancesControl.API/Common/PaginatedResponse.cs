namespace FinancesControl.API.Common
{
    public class PaginatedResponse<T> : BaseResponse
    {
        public List<T> Data { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPrevPage { get; set; }

        public void SetPagination(int totalPages, int currentPage, bool hasNextPage, bool hasPrevPage)
        {
            TotalPages = totalPages;
            CurrentPage = currentPage;
            HasNextPage = hasNextPage;
            HasPrevPage = hasPrevPage;
        }
    }
}
