namespace FinancesControl.API.Common
{
    public abstract class BaseResponse
    {
        public List<string> Errors { get; private set; } = new List<string>();
        public int HttpStatusCode { get; private set; } = 200;
        public void AddError(string error)
        {
            Errors.Add(error);
        }

        public void AddErrors(List<string> errors)
        {
            Errors.AddRange(errors);
        }

        public void SetHttpStatusCode(int httpStatusCode)
        {
            HttpStatusCode = httpStatusCode;
        }
    }
}
