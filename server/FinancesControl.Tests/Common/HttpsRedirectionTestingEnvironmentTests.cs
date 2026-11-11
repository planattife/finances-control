using System.Collections.Concurrent;
using FinancesControl.API.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace FinancesControl.Tests.Common;

public class HttpsRedirectionTestingEnvironmentTests
{
    private const string HttpsPortWarning = "Failed to determine the https port for redirect";

    [Fact]
    public async Task TestingEnvironment_WithHttpRequest_DoesNotLogHttpsPortWarning()
    {
        var warningMessages = new ConcurrentQueue<string>();
        using var loggerProvider = new WarningCaptureLoggerProvider(warningMessages);
        using var factory = CreateFactory(loggerProvider);
        using var client = factory.CreateClient();

        _ = await client.GetAsync("/manage/info");

        warningMessages
            .Any(message => message.Contains(HttpsPortWarning, StringComparison.OrdinalIgnoreCase))
            .Should()
            .BeFalse();
    }

    private static WebApplicationFactory<Program> CreateFactory(ILoggerProvider loggerProvider)
    {
        var databaseName = $"FinancesControlHttpsRedirectionTests-{Guid.NewGuid()}";

        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureLogging(logging =>
                {
                    logging.AddProvider(loggerProvider);
                });
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<DbContextOptions<FinancesDbContext>>();
                    services.AddDbContext<FinancesDbContext>(options =>
                        options.UseInMemoryDatabase(databaseName));
                });
            });
    }

    private sealed class WarningCaptureLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentQueue<string> _warningMessages;

        public WarningCaptureLoggerProvider(ConcurrentQueue<string> warningMessages)
        {
            _warningMessages = warningMessages;
        }

        public ILogger CreateLogger(string categoryName)
            => new WarningCaptureLogger(_warningMessages);

        public void Dispose()
        {
        }
    }

    private sealed class WarningCaptureLogger : ILogger
    {
        private readonly ConcurrentQueue<string> _warningMessages;

        public WarningCaptureLogger(ConcurrentQueue<string> warningMessages)
        {
            _warningMessages = warningMessages;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
            => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel)
            => logLevel >= LogLevel.Warning;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            _warningMessages.Enqueue(formatter(state, exception));
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }
}
