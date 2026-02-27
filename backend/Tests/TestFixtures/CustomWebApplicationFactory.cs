using Backend.Application.Services;
using Backend.Infrastructure.Configurations;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Tests.TestFixtures;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        SharedMongoDbInstance.EnsureStarted();
        var connectionString = SharedMongoDbInstance.ConnectionString;
        
        Environment.SetEnvironmentVariable("ENABLE_REQUEST_TRACING", "1");
        Environment.SetEnvironmentVariable("MONGODB_CONNECTION_STRING", connectionString);
        Environment.SetEnvironmentVariable("MONGODB_NAME", "IntegrationTestsDb");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoDB:ConnectionString"] = connectionString,
                ["MongoDB:Name"] = "IntegrationTestsDb",
                ["JWT:SecretKey"] = "test-secret-key-that-is-at-least-32-characters-long",
                ["JWT:Issuer"] = "test-issuer",
                ["JWT:Audience"] = "test-audience",
                ["JWT:ExpiresInMinutes"] = "60",
                ["SMTP:Server"] = "localhost",
                ["SMTP:Port"] = "25",
                ["SMTP:SenderEmail"] = "test@example.com",
                ["SMTP:SenderPassword"] = "test-password",
                ["RateLimit:RequestLimit"] = "1000",
                ["RateLimit:WindowSeconds"] = "60",
                ["Outbox:PollingIntervalMs"] = "200"
            });
        });

        builder.ConfigureServices(services =>
        {
            StronglyTypedIdSerializationRegistry.Register();

            var emailServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IEmailService));
            if (emailServiceDescriptor != null)
            {
                services.Remove(emailServiceDescriptor);
            }
            var mockEmailService = new Mock<IEmailService>();
            services.AddSingleton(mockEmailService.Object);

            var rateLimiterDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IRateLimiter));
            if (rateLimiterDescriptor != null)
            {
                services.Remove(rateLimiterDescriptor);
            }
            services.AddSingleton<IRateLimiter, NoopTestRateLimiter>();
        });
    }

    public async Task InitializeAsync()
    {
        _ = Server;
        await SharedMongoDbInstance.WaitForReadyAsync();
    }

    public new async Task DisposeAsync()
    {
        SharedMongoDbInstance.Release();
        await base.DisposeAsync();
    }

    private sealed class NoopTestRateLimiter : IRateLimiter
    {
        public Task<bool> ShouldAllowAsync(string key, int limit, TimeSpan window, CancellationToken ct = default)
            => Task.FromResult(true);
    }
}
