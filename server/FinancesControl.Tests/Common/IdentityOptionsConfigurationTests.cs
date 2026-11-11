using FinancesControl.API.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;

namespace FinancesControl.Tests.Common;

public class IdentityOptionsConfigurationTests
{
    [Fact]
    public void Configure_WithDefaultOptions_SetsProductionPasswordOptions()
    {
        var options = new IdentityOptions();

        IdentityOptionsConfiguration.Configure(options);

        options.Password.RequiredLength.Should().Be(12);
        options.Password.RequiredUniqueChars.Should().Be(4);
        options.Password.RequireDigit.Should().BeTrue();
        options.Password.RequireLowercase.Should().BeTrue();
        options.Password.RequireUppercase.Should().BeTrue();
        options.Password.RequireNonAlphanumeric.Should().BeTrue();
    }

    [Fact]
    public void Configure_WithDefaultOptions_SetsProductionLockoutOptions()
    {
        var options = new IdentityOptions();

        IdentityOptionsConfiguration.Configure(options);

        options.Lockout.DefaultLockoutTimeSpan.Should().Be(TimeSpan.FromMinutes(15));
        options.Lockout.MaxFailedAccessAttempts.Should().Be(5);
        options.Lockout.AllowedForNewUsers.Should().BeTrue();
    }

    [Fact]
    public void Configure_WithDefaultOptions_RequiresUniqueEmail()
    {
        var options = new IdentityOptions();

        IdentityOptionsConfiguration.Configure(options);

        options.User.RequireUniqueEmail.Should().BeTrue();
    }

    [Fact]
    public void Configure_WithDefaultOptions_LeavesConfirmedAccountDisabledUntilEmailConfirmationExists()
    {
        var options = new IdentityOptions();

        IdentityOptionsConfiguration.Configure(options);

        options.SignIn.RequireConfirmedEmail.Should().BeFalse();
        options.SignIn.RequireConfirmedAccount.Should().BeFalse();
        options.SignIn.RequireConfirmedPhoneNumber.Should().BeFalse();
    }
}
