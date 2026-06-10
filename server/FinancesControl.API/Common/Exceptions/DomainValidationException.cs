namespace FinancesControl.API.Common;

public class DomainValidationException : Exception
{
    public DomainValidationException(string message) : base(message)
    {
    }
}
