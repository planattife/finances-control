using FinancesControl.API.Common.Antiforgery;
using FluentAssertions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace FinancesControl.Tests.Common;

public class ApiAntiforgeryConfigurationTests
{
    [Fact]
    public void Configure_WithDefaultOptions_SetsExpectedAntiforgeryOptions()
    {
        var options = new AntiforgeryOptions();

        ApiAntiforgeryConfiguration.Configure(options);

        ApiAntiforgeryConfiguration.TokenCookieName.Should().Be("XSRF-TOKEN");
        ApiAntiforgeryConfiguration.TokenHeaderName.Should().Be("X-XSRF-TOKEN");
        options.HeaderName.Should().Be(ApiAntiforgeryConfiguration.TokenHeaderName);
        options.Cookie.Name.Should().Be(".FinancesControl.Antiforgery");
        options.Cookie.HttpOnly.Should().BeTrue();
        options.Cookie.SameSite.Should().Be(SameSiteMode.Strict);
        options.Cookie.SecurePolicy.Should().Be(CookieSecurePolicy.SameAsRequest);
    }

    [Fact]
    public void CreateRequestTokenCookieOptions_WithDefaultOptions_ReturnsReadableTokenCookieOptions()
    {
        var options = ApiAntiforgeryConfiguration.CreateRequestTokenCookieOptions();

        options.HttpOnly.Should().BeFalse();
        options.SameSite.Should().Be(SameSiteMode.Strict);
        options.Secure.Should().BeFalse();
    }
}
