using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace FinancesControl.API.Common.RateLimiting;

public static class ApiRateLimitingConfiguration
{
    internal const string RejectionMessage = "Too many requests. Please try again later.";
    private const string UnknownPartitionKey = "unknown";

    public static void Configure(RateLimiterOptions options, ApiRateLimitOptions limits)
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.OnRejected = WriteRejectedResponse;

        options.AddPolicy(RateLimitPolicyNames.Auth, httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                GetAuthPartitionKey(httpContext),
                _ => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = limits.AuthPermitLimit,
                    QueueLimit = limits.AuthQueueLimit,
                    Window = limits.AuthWindow
                }));

        options.AddPolicy(RateLimitPolicyNames.FinanceWrite, httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                GetFinanceWritePartitionKey(httpContext),
                _ => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = limits.FinanceWritePermitLimit,
                    QueueLimit = limits.FinanceWriteQueueLimit,
                    Window = limits.FinanceWriteWindow
                }));
    }

    internal static string GetAuthPartitionKey(HttpContext httpContext)
    {
        return httpContext.Connection.RemoteIpAddress?.ToString() ?? UnknownPartitionKey;
    }

    internal static string GetFinanceWritePartitionKey(HttpContext httpContext)
    {
        return httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? GetAuthPartitionKey(httpContext);
    }

    private static async ValueTask WriteRejectedResponse(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var retryAfter = context.Lease.MetadataNames
            .Select(metadataName => context.Lease.TryGetMetadata(metadataName, out var metadata) ? metadata : null)
            .OfType<TimeSpan>()
            .FirstOrDefault();

        var response = new ErrorResponse();
        response.SetHttpStatusCode(StatusCodes.Status429TooManyRequests);
        response.AddError(RejectionMessage);

        context.HttpContext.Response.Clear();
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        if (retryAfter > TimeSpan.Zero)
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.HttpContext.Response.WriteAsync(json, cancellationToken);
    }
}
