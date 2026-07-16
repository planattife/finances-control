using System.Net;
using System.Net.Http.Json;
using FinancesControl.API.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FinancesControl.Tests.Common;

public class IdentityEndpointAuthorizationTests
{
    private const string TestEmail = "identity-endpoints@test.local";
    private const string TestPassword = "ValidPass123!";

    [Fact]
    public async Task Login_WithAnonymousRequest_ReachesIdentityLoginEndpoint()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/login?useCookies=true", new { });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("/register")]
    [InlineData("/refresh")]
    [InlineData("/forgotPassword")]
    [InlineData("/resetPassword")]
    [InlineData("/resendConfirmationEmail")]
    [InlineData("/manage/info")]
    [InlineData("/manage/2fa")]
    public async Task RestrictedPostIdentityEndpoints_WithAnonymousRequest_ReturnUnauthorized(string endpoint)
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(endpoint, new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ConfirmEmail_WithAnonymousRequest_ReturnsUnauthorized()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/confirmEmail");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ManageInfo_WithAnonymousRequest_ReturnsUnauthorized()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/manage/info");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ManageInfo_WithAuthenticatedUser_IsNotRejectedByAuthorization()
    {
        using var factory = CreateFactory();
        await SeedUser(factory);

        using var client = factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync(
            "/login?useCookies=true",
            new { email = TestEmail, password = TestPassword });

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await client.GetAsync("/manage/info");

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ManageInfoPost_WithAuthenticatedUser_ReturnsForbidden()
    {
        using var factory = CreateFactory();
        await SeedUser(factory);

        using var client = factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync(
            "/login?useCookies=true",
            new { email = TestEmail, password = TestPassword });

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await client.PostAsJsonAsync("/manage/info", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        var databaseName = $"FinancesControlIdentityTests-{Guid.NewGuid()}";

        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<DbContextOptions<FinancesDbContext>>();
                    services.AddDbContext<FinancesDbContext>(options =>
                        options.UseInMemoryDatabase(databaseName));
                });
            });
    }

    private static async Task SeedUser(WebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        var user = new IdentityUser
        {
            UserName = TestEmail,
            Email = TestEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, TestPassword);

        result.Succeeded.Should().BeTrue(string.Join(", ", result.Errors.Select(error => error.Description)));
    }
}
