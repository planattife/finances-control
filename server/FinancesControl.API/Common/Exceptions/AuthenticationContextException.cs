namespace FinancesControl.API.Common;

public class AuthenticationContextException : Exception
{
    public AuthenticationContextException(string message) : base(message)
    {
    }
}
