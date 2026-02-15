namespace FinancesControl.API.Common;

public static class ValidationHelper
{
    public static void ValidateName(string name, int minLength = 2, int maxLength = 50)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.");

        if (name.Length < minLength)
            throw new ArgumentException($"Name must be at least {minLength} characters long.");

        if (name.Length > maxLength)
            throw new ArgumentException($"Name cannot exceed {maxLength} characters.");
    }
}