using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;

namespace FinancesControl.API.Common;

public static class IdentityEndpointRouteBuilderExtensions
{
    private const string LoginPath = "/login";
    private const string ManageInfoPath = "/manage/info";
    private const string RestrictedIdentityEndpointRole = "FinancesControl.UnsupportedIdentityEndpoint";

    public static IEndpointConventionBuilder MapRestrictedIdentityApi<TUser>(this IEndpointRouteBuilder endpoints)
        where TUser : class, new()
    {
        var builder = endpoints.MapIdentityApi<TUser>();

        builder.Add(endpointBuilder =>
        {
            if (endpointBuilder is not RouteEndpointBuilder routeEndpointBuilder)
                return;

            var routePattern = routeEndpointBuilder.RoutePattern.RawText;
            var httpMethods = endpointBuilder.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault()?.HttpMethods;

            if (IsAllowedIdentityEndpoint(routePattern, httpMethods))
                return;

            endpointBuilder.Metadata.Add(new AuthorizeAttribute { Roles = RestrictedIdentityEndpointRole });
        });

        return builder;
    }

    internal static bool IsAllowedIdentityEndpoint(string? routePattern, IReadOnlyList<string>? httpMethods)
    {
        if (string.IsNullOrWhiteSpace(routePattern) || httpMethods is null)
            return false;

        var normalizedRoutePattern = NormalizeRoutePattern(routePattern);

        return IsHttpMethod(httpMethods, HttpMethods.Post) && normalizedRoutePattern == LoginPath
            || IsHttpMethod(httpMethods, HttpMethods.Get) && normalizedRoutePattern == ManageInfoPath;
    }

    private static string NormalizeRoutePattern(string routePattern)
    {
        var trimmedRoutePattern = routePattern.Trim();

        return trimmedRoutePattern.StartsWith('/')
            ? trimmedRoutePattern
            : $"/{trimmedRoutePattern}";
    }

    private static bool IsHttpMethod(IReadOnlyList<string> httpMethods, string expectedMethod)
    {
        return httpMethods.Any(method => string.Equals(method, expectedMethod, StringComparison.OrdinalIgnoreCase));
    }
}
