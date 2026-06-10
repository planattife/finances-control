namespace FinancesControl.API.Common;

public static class ValidationHelper
{
    public static void ValidateName(string name, int minLength = 2, int maxLength = 50)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Name cannot be null or empty.");

        if (name.Length < minLength)
            throw new DomainValidationException($"Name must be at least {minLength} characters long.");

        if (name.Length > maxLength)
            throw new DomainValidationException($"Name cannot exceed {maxLength} characters.");
    }
}
