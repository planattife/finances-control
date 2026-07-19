using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FinancesControl.API.Common.Antiforgery;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ValidateAntiforgeryTokenAttribute : Attribute, IAsyncAuthorizationFilter
{
    internal const string InvalidTokenMessage = "Invalid or missing CSRF token.";

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (HttpMethods.IsGet(context.HttpContext.Request.Method)
            || HttpMethods.IsHead(context.HttpContext.Request.Method)
            || HttpMethods.IsOptions(context.HttpContext.Request.Method)
            || HttpMethods.IsTrace(context.HttpContext.Request.Method))
        {
            return;
        }

        var antiforgery = context.HttpContext.RequestServices.GetRequiredService<IAntiforgery>();

        try
        {
            await antiforgery.ValidateRequestAsync(context.HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            var response = new ErrorResponse();
            response.SetHttpStatusCode(StatusCodes.Status400BadRequest);
            response.AddError(InvalidTokenMessage);

            context.Result = new BadRequestObjectResult(response);
        }
    }
}
