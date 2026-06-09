using System.Net;
using System.Text.Json;

namespace FinancesControl.API.Common;

public class GlobalExceptionHandlerMiddleware
{
    private const string UnexpectedErrorMessage = "An unexpected error occurred.";
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, shouldLogAsError) = exception switch
        {
            AuthenticationContextException => ((int)HttpStatusCode.Unauthorized, exception.Message, false),
            ResourceNotFoundException => ((int)HttpStatusCode.NotFound, exception.Message, false),
            DomainValidationException => ((int)HttpStatusCode.BadRequest, exception.Message, false),
            BusinessRuleException => ((int)HttpStatusCode.BadRequest, exception.Message, false),
            _ => ((int)HttpStatusCode.InternalServerError, UnexpectedErrorMessage, true)
        };

        if (shouldLogAsError)
            _logger.LogError(exception, "Unhandled exception while processing the request.");
        else
            _logger.LogInformation(exception, "Handled application exception while processing the request.");

        var response = new ErrorResponse();
        response.SetHttpStatusCode(statusCode);
        response.AddError(message);

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
