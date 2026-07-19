using FinancesControl.API.Common;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace FinancesControl.Tests.Common;

public class ProductionConfigurationTests
{
    private const string SecretConnectionString = "Host=prod;Username=fcuser;Password=SuperSecret;Database=FinancesControl";

    [Fact]
    public void Validate_WithProductionConfig_ReturnsNormalizedValues()
    {
        var configuration = CreateConfiguration(
            new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", SecretConnectionString),
            new KeyValuePair<string, string?>("Cors:AllowedOrigins:0", " https://app.example.com "),
            new KeyValuePair<string, string?>("Cors:AllowedOrigins:1", "https://admin.example.com"),
            new KeyValuePair<string, string?>("AllowedHosts", "api.example.com;admin-api.example.com"));
        var environment = CreateEnvironment("Production");

        var productionConfiguration = ProductionConfigurationValidator.Validate(configuration, environment);

        productionConfiguration.DefaultConnection.Should().Be(SecretConnectionString);
        productionConfiguration.CorsAllowedOrigins.Should().Equal("https://app.example.com", "https://admin.example.com");
    }

    [Fact]
    public void Validate_WithMissingDefaultConnectionInProduction_ThrowsInvalidOperationException()
    {
        var configuration = CreateConfiguration(
            new KeyValuePair<string, string?>("Cors:AllowedOrigins:0", "https://app.example.com"),
            new KeyValuePair<string, string?>("AllowedHosts", "api.example.com"));
        var environment = CreateEnvironment("Production");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ProductionConfigurationValidator.Validate(configuration, environment));

        exception.Message.Should().Contain("ConnectionStrings:DefaultConnection");
    }

    [Fact]
    public void Validate_WithMissingCorsOriginsInProduction_ThrowsInvalidOperationException()
    {
        var configuration = CreateConfiguration(
            new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", SecretConnectionString),
            new KeyValuePair<string, string?>("AllowedHosts", "api.example.com"));
        var environment = CreateEnvironment("Production");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ProductionConfigurationValidator.Validate(configuration, environment));

        exception.Message.Should().Contain("Cors:AllowedOrigins");
        exception.Message.Should().NotContain(SecretConnectionString);
    }

    [Fact]
    public void Validate_WithWildcardAllowedHostsInProduction_ThrowsInvalidOperationException()
    {
        var configuration = CreateConfiguration(
            new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", SecretConnectionString),
            new KeyValuePair<string, string?>("Cors:AllowedOrigins:0", "https://app.example.com"),
            new KeyValuePair<string, string?>("AllowedHosts", "*"));
        var environment = CreateEnvironment("Production");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ProductionConfigurationValidator.Validate(configuration, environment));

        exception.Message.Should().Contain("AllowedHosts");
        exception.Message.Should().NotContain(SecretConnectionString);
    }

    [Fact]
    public void Validate_WithWildcardCorsOriginInProduction_ThrowsInvalidOperationException()
    {
        var configuration = CreateConfiguration(
            new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", SecretConnectionString),
            new KeyValuePair<string, string?>("Cors:AllowedOrigins:0", "*"),
            new KeyValuePair<string, string?>("AllowedHosts", "api.example.com"));
        var environment = CreateEnvironment("Production");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ProductionConfigurationValidator.Validate(configuration, environment));

        exception.Message.Should().Contain("Cors:AllowedOrigins");
        exception.Message.Should().NotContain(SecretConnectionString);
    }

    [Fact]
    public void Validate_WithTrailingSlashCorsOriginInProduction_ThrowsInvalidOperationException()
    {
        var configuration = CreateConfiguration(
            new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", SecretConnectionString),
            new KeyValuePair<string, string?>("Cors:AllowedOrigins:0", "https://app.example.com/"),
            new KeyValuePair<string, string?>("AllowedHosts", "api.example.com"));
        var environment = CreateEnvironment("Production");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ProductionConfigurationValidator.Validate(configuration, environment));

        exception.Message.Should().Contain("Cors:AllowedOrigins");
    }

    [Fact]
    public void Validate_WithTestingEnvironment_AllowsMissingProductionValues()
    {
        var configuration = CreateConfiguration();
        var environment = CreateEnvironment("Testing");

        var productionConfiguration = ProductionConfigurationValidator.Validate(configuration, environment);

        productionConfiguration.DefaultConnection.Should().BeEmpty();
        productionConfiguration.CorsAllowedOrigins.Should().BeEmpty();
    }

    private static IConfiguration CreateConfiguration(params KeyValuePair<string, string?>[] values)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

    private static IHostEnvironment CreateEnvironment(string environmentName)
        => new TestHostEnvironment
        {
            EnvironmentName = environmentName,
            ApplicationName = "FinancesControl.Tests",
            ContentRootPath = Directory.GetCurrentDirectory(),
            ContentRootFileProvider = new NullFileProvider()
        };

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = string.Empty;
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
