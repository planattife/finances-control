using FinancesControl.API.Common;
using FluentAssertions;

namespace FinancesControl.Tests.Common;

public class IdentityEndpointRouteBuilderExtensionsTests
{
    [Theory]
    [InlineData("/login", "POST")]
    [InlineData("login", "post")]
    [InlineData("/manage/info", "GET")]
    [InlineData("manage/info", "get")]
    public void IsAllowedIdentityEndpoint_WithAllowedEndpoint_ReturnsTrue(string routePattern, string httpMethod)
    {
        var isAllowed = IdentityEndpointRouteBuilderExtensions.IsAllowedIdentityEndpoint(
            routePattern,
            [httpMethod]);

        isAllowed.Should().BeTrue();
    }

    [Theory]
    [InlineData("/register", "POST")]
    [InlineData("/refresh", "POST")]
    [InlineData("/forgotPassword", "POST")]
    [InlineData("/resetPassword", "POST")]
    [InlineData("/confirmEmail", "GET")]
    [InlineData("/resendConfirmationEmail", "POST")]
    [InlineData("/manage/info", "POST")]
    [InlineData("/manage/2fa", "POST")]
    public void IsAllowedIdentityEndpoint_WithRestrictedEndpoint_ReturnsFalse(string routePattern, string httpMethod)
    {
        var isAllowed = IdentityEndpointRouteBuilderExtensions.IsAllowedIdentityEndpoint(
            routePattern,
            [httpMethod]);

        isAllowed.Should().BeFalse();
    }
}
