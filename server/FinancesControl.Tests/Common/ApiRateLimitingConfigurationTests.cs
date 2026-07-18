using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.RateLimiting;
using FinancesControl.API.Common.RateLimiting;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace FinancesControl.Tests.Common;

public class ApiRateLimitingConfigurationTests
{
    [Fact]
    public void Configure_WithDefaultOptions_SetsExpectedRateLimiterDefaults()
    {
        var limits = new ApiRateLimitOptions();
        var options = new RateLimiterOptions();

        ApiRateLimitingConfiguration.Configure(options, limits);

        options.RejectionStatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
        options.OnRejected.Should().NotBeNull();
        limits.AuthPermitLimit.Should().Be(5);
        limits.AuthWindow.Should().Be(TimeSpan.FromMinutes(1));
        limits.AuthQueueLimit.Should().Be(0);
        limits.FinanceWritePermitLimit.Should().Be(60);
        limits.FinanceWriteWindow.Should().Be(TimeSpan.FromMinutes(1));
        limits.FinanceWriteQueueLimit.Should().Be(0);
        RateLimitPolicyNames.Auth.Should().Be("auth");
        RateLimitPolicyNames.FinanceWrite.Should().Be("finance-write");
    }

    [Fact]
    public void GetAuthPartitionKey_WithDifferentRemoteIps_ReturnsDifferentPartitions()
    {
        var firstContext = CreateContext("127.0.0.1");
        var secondContext = CreateContext("127.0.0.2");

        var firstPartition = ApiRateLimitingConfiguration.GetAuthPartitionKey(firstContext);
        var secondPartition = ApiRateLimitingConfiguration.GetAuthPartitionKey(secondContext);

        firstPartition.Should().Be("127.0.0.1");
        secondPartition.Should().Be("127.0.0.2");
        firstPartition.Should().NotBe(secondPartition);
    }

    [Fact]
    public void GetAuthPartitionKey_WithMissingRemoteIp_ReturnsUnknownPartition()
    {
        var context = new DefaultHttpContext();

        var partition = ApiRateLimitingConfiguration.GetAuthPartitionKey(context);

        partition.Should().Be("unknown");
    }

    [Fact]
    public void GetFinanceWritePartitionKey_WithAuthenticatedUser_ReturnsUserIdPartition()
    {
        var context = CreateContext("127.0.0.1", "user-123");

        var partition = ApiRateLimitingConfiguration.GetFinanceWritePartitionKey(context);

        partition.Should().Be("user-123");
    }

    [Fact]
    public void GetFinanceWritePartitionKey_WithMissingUserId_ReturnsRemoteIpPartition()
    {
        var context = CreateContext("127.0.0.1");

        var partition = ApiRateLimitingConfiguration.GetFinanceWritePartitionKey(context);

        partition.Should().Be("127.0.0.1");
    }

    [Fact]
    public async Task OnRejected_WithRejectedLease_ReturnsJsonTooManyRequestsResponse()
    {
        var options = new RateLimiterOptions();
        ApiRateLimitingConfiguration.Configure(options, new ApiRateLimitOptions());

        var httpContext = CreateContext("127.0.0.1");
        httpContext.Response.Body = new MemoryStream();

        var rejectedContext = new OnRejectedContext
        {
            HttpContext = httpContext,
            Lease = new RejectedLease(TimeSpan.FromSeconds(30))
        };

        await options.OnRejected!(rejectedContext, CancellationToken.None);

        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
        httpContext.Response.ContentType.Should().Be("application/json");
        httpContext.Response.Headers.RetryAfter.ToString().Should().Be("30");

        var response = await ReadResponse(httpContext);
        response.RootElement.GetProperty("httpStatusCode").GetInt32().Should().Be(StatusCodes.Status429TooManyRequests);
        response.RootElement.GetProperty("errors")[0].GetString().Should().Be(ApiRateLimitingConfiguration.RejectionMessage);
    }

    private static DefaultHttpContext CreateContext(string remoteIp, string? userId = null)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(remoteIp);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            context.User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId)],
                authenticationType: "Test"));
        }

        return context;
    }

    private static async Task<JsonDocument> ReadResponse(DefaultHttpContext context)
    {
        context.Response.Body.Position = 0;
        return await JsonDocument.ParseAsync(context.Response.Body);
    }

    private sealed class RejectedLease : RateLimitLease
    {
        private readonly TimeSpan _retryAfter;

        public RejectedLease(TimeSpan retryAfter)
        {
            _retryAfter = retryAfter;
        }

        public override bool IsAcquired => false;

        public override IEnumerable<string> MetadataNames => [MetadataName.RetryAfter.Name];

        public override bool TryGetMetadata(string metadataName, out object? metadata)
        {
            metadata = _retryAfter;
            return true;
        }

    }
}
