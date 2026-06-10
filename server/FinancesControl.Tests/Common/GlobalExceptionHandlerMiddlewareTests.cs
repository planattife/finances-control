using System.Text.Json;
using FinancesControl.API.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace FinancesControl.Tests.Common;

public class GlobalExceptionHandlerMiddlewareTests
{
    [Theory]
    [InlineData(typeof(DomainValidationException), 400, "Invalid amount.")]
    [InlineData(typeof(BusinessRuleException), 400, "Card is inactive.")]
    [InlineData(typeof(ResourceNotFoundException), 404, "Account not found.")]
    [InlineData(typeof(AuthenticationContextException), 401, "User context is not available.")]
    public async Task InvokeAsync_WithApplicationException_ReturnsMappedErrorResponse(Type exceptionType, int statusCode, string message)
    {
        var exception = (Exception)Activator.CreateInstance(exceptionType, message)!;

        var context = await InvokeMiddleware(exception);

        context.Response.StatusCode.Should().Be(statusCode);
        context.Response.ContentType.Should().Be("application/json");

        var response = await ReadResponse(context);
        response.RootElement.GetProperty("httpStatusCode").GetInt32().Should().Be(statusCode);
        response.RootElement.GetProperty("errors")[0].GetString().Should().Be(message);

        var body = await ReadBody(context);
        body.Should().NotContain("StackTrace");
        body.Should().NotContain(exceptionType.Name);
    }

    [Fact]
    public async Task InvokeAsync_WithUnexpectedException_ReturnsGenericErrorWithoutDetails()
    {
        const string internalMessage = "Database exploded with sensitive details.";

        var context = await InvokeMiddleware(new InvalidOperationException(internalMessage));

        context.Response.StatusCode.Should().Be(500);

        var body = await ReadBody(context);
        body.Should().Contain("An unexpected error occurred.");
        body.Should().NotContain(internalMessage);
        body.Should().NotContain("InvalidOperationException");
        body.Should().NotContain("StackTrace");
    }

    private static async Task<DefaultHttpContext> InvokeMiddleware(Exception exception)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw exception,
            NullLogger<GlobalExceptionHandlerMiddleware>.Instance);

        await middleware.InvokeAsync(context);
        context.Response.Body.Position = 0;

        return context;
    }

    private static async Task<JsonDocument> ReadResponse(DefaultHttpContext context)
    {
        var body = await ReadBody(context);
        return JsonDocument.Parse(body);
    }

    private static async Task<string> ReadBody(DefaultHttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.Response.Body.Position = 0;
        return body;
    }
}
