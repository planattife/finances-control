using System.Net;
using System.Net.Http.Json;
using FinancesControl.API.Common.Antiforgery;
using FinancesControl.API.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FinancesControl.Tests.Common;

public class ApiAntiforgeryTests
{
    private const string TestEmail = "api-antiforgery@test.local";
    private const string TestPassword = "ValidPass123!";

    [Fact]
    public async Task Token_WithAnonymousRequest_ReturnsUnauthorized()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/antiforgery/token");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Token_WithAuthenticatedRequest_SetsReadableXsrfTokenCookie()
    {
        using var factory = CreateFactory();
        await SeedUser(factory);
        using var client = factory.CreateClient();
        await Login(client);

        var response = await client.GetAsync("/antiforgery/token");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        GetCookieValue(response, ApiAntiforgeryConfiguration.TokenCookieName).Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task FinanceWrite_WithAuthenticatedRequestWithoutToken_ReturnsBadRequest()
    {
        using var factory = CreateFactory();
        await SeedUser(factory);
        using var client = factory.CreateClient();
        await Login(client);

        var response = await client.PostAsJsonAsync("/api/Account", new { name = "Personal wallet" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        error!.HttpStatusCode.Should().Be(StatusCodes.Status400BadRequest);
        error.Errors.Should().Contain(ValidateAntiforgeryTokenAttribute.InvalidTokenMessage);
    }

    [Fact]
    public async Task Logout_WithAuthenticatedRequestWithoutToken_ReturnsBadRequest()
    {
        using var factory = CreateFactory();
        await SeedUser(factory);
        using var client = factory.CreateClient();
        await Login(client);

        var response = await client.PostAsync("/logout", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Logout_WithAuthenticatedRequestAndValidToken_ReturnsNoContent()
    {
        using var factory = CreateFactory();
        await SeedUser(factory);
        using var client = factory.CreateClient();
        await Login(client);
        var token = await GetAntiforgeryToken(client);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/logout");
        request.Headers.Add(ApiAntiforgeryConfiguration.TokenHeaderName, token);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ManageInfo_WithAuthenticatedRequestWithoutToken_IsNotRejectedByAntiforgery()
    {
        using var factory = CreateFactory();
        await SeedUser(factory);
        using var client = factory.CreateClient();
        await Login(client);

        var response = await client.GetAsync("/manage/info");

        response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithAnonymousRequestWithoutToken_ReachesIdentityLoginEndpoint()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/login?useCookies=true", new { });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static WebApplicationFactory<Program> CreateFactory(string environment = "Testing")
    {
        var databaseName = $"FinancesControlAntiforgeryTests-{Guid.NewGuid()}";

        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment(environment);
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

    private static async Task Login(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/login?useCookies=true",
            new { email = TestEmail, password = TestPassword });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task<string> GetAntiforgeryToken(HttpClient client)
    {
        var response = await client.GetAsync("/antiforgery/token");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var token = GetCookieValue(response, ApiAntiforgeryConfiguration.TokenCookieName);
        token.Should().NotBeNullOrWhiteSpace();

        return token!;
    }

    private static string? GetCookieValue(HttpResponseMessage response, string cookieName)
    {
        var setCookieHeader = response.Headers.TryGetValues("Set-Cookie", out var values)
            ? values.FirstOrDefault(value => value.StartsWith($"{cookieName}=", StringComparison.Ordinal))
            : null;

        return setCookieHeader?.Split(';')[0].Split('=', 2)[1];
    }

    private sealed class ApiErrorResponse
    {
        public int HttpStatusCode { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}
