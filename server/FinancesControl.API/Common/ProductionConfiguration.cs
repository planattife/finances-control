using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace FinancesControl.API.Common;

public sealed record ProductionConfiguration(string DefaultConnection, string[] CorsAllowedOrigins);

public static class ProductionConfigurationValidator
{
    private const string DefaultConnectionKey = "ConnectionStrings:DefaultConnection";
    private const string CorsAllowedOriginsKey = "Cors:AllowedOrigins";
    private const string AllowedHostsKey = "AllowedHosts";

    public static ProductionConfiguration Validate(IConfiguration configuration, IHostEnvironment environment)
    {
        var defaultConnection = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        var corsAllowedOrigins = GetCorsAllowedOrigins(configuration);

        if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
            return new ProductionConfiguration(defaultConnection, corsAllowedOrigins);

        ValidateRequired(defaultConnection, DefaultConnectionKey);
        ValidateRequiredOrigins(corsAllowedOrigins);
        ValidateAllowedHosts(configuration[AllowedHostsKey]);

        return new ProductionConfiguration(defaultConnection, corsAllowedOrigins);
    }

    private static string[] GetCorsAllowedOrigins(IConfiguration configuration)
        => configuration
            .GetSection(CorsAllowedOriginsKey)
            .Get<string[]>()?
            .Select(origin => origin.Trim())
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .ToArray() ?? [];

    private static void ValidateRequired(string? value, string key)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Production configuration requires '{key}'.");
    }

    private static void ValidateRequiredOrigins(string[] origins)
    {
        if (origins.Length == 0)
            throw new InvalidOperationException($"Production configuration requires at least one '{CorsAllowedOriginsKey}' value.");

        foreach (var origin in origins)
        {
            if (origin == "*")
                throw new InvalidOperationException($"Production configuration '{CorsAllowedOriginsKey}' cannot contain wildcard origins.");

            if (origin.EndsWith('/'))
                throw new InvalidOperationException($"Production configuration '{CorsAllowedOriginsKey}' origins must not end with '/'.");
        }
    }

    private static void ValidateAllowedHosts(string? allowedHosts)
    {
        ValidateRequired(allowedHosts, AllowedHostsKey);

        var hosts = allowedHosts!
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (hosts.Length == 0 || hosts.Any(host => host == "*"))
            throw new InvalidOperationException($"Production configuration '{AllowedHostsKey}' cannot be wildcard or empty.");
    }
}
