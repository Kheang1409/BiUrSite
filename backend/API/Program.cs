using Backend.API.Middleware;
using Backend.API.Extensions;
using Backend.API.GraphQL;
using Backend.Application;
using Backend.Infrastructure;
using Backend.Infrastructure.Authentication;
using Backend.Infrastructure.Configuration;
using Backend.Infrastructure.Extensions;
using Backend.Infrastructure.Configurations;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024;
    options.Limits.MaxRequestHeadersTotalSize = 32 * 1024;
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
});

StronglyTypedIdSerializationRegistry.Register();

builder.Services.AddHttpContextAccessor();
builder.Services.AddCorrelationId();
builder.Services.AddScoped<GraphQLUserContext>();
builder.Services.ConfigureAuthenticationServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCorsPolicies(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddHealthChecks();

builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddErrorFilter<GraphQLErrorFilter>()
    .ModifyRequestOptions(o => o.IncludeExceptionDetails = builder.Environment.IsDevelopment());

var app = builder.Build();

await app.InitializeDatabaseAsync();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseSecurityHeaders();
app.UseCorrelationId();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseWebSockets();

app.UseRouting();

app.UseCors(CorsConfiguration.AllowFrontendPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.UseDiagnostics();

app.MapHealthChecks("/health");
app.MapEndpoints();

app.Run();

public partial class Program { }