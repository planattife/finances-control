using Microsoft.AspNetCore.Antiforgery;

namespace FinancesControl.API.Common.Antiforgery;

public static class ApiAntiforgeryConfiguration
{
    public const string TokenCookieName = "XSRF-TOKEN";
    public const string TokenHeaderName = "X-XSRF-TOKEN";
    internal const string AntiforgeryCookieName = ".FinancesControl.Antiforgery";

    public static void Configure(AntiforgeryOptions options)
    {
        options.HeaderName = TokenHeaderName;
        options.Cookie.Name = AntiforgeryCookieName;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    }

    public static CookieOptions CreateRequestTokenCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = false,
            SameSite = SameSiteMode.Strict,
            Secure = false
        };
    }
}
