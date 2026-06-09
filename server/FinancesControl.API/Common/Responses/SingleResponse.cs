namespace FinancesControl.API.Common
{
    public class SingleResponse<T> : BaseResponse
    {
        public T Data { get; set; }
    }
}
