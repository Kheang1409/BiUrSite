using Backend.API.Middleware;
using Backend.Application;
using Backend.Infrastructure;
using Backend.Infrastructure.Authentication;
using Backend.Infrastructure.Configuration;
using Backend.Infrastructure.Hubs;
using Backend.Infrastructure.Swagger;
using Backend.Infrastructure.Configurations;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

StronglyTypedIdSerializationRegistry.Register();

builder.Services.AddControllers();
builder.Services.ConfigureAuthenticationServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCorsPolicies(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddSignalR();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();
// Development-only middleware in one place (container runs HTTP behind compose/proxy)

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BiUrSite API v1");
});

app.UseRouting();

app.UseCors(CorsConfiguration.AllowAllPolicy);

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHub<FeedHub>("/feedHub")
    .RequireCors(CorsConfiguration.AllowFrontendPolicy);
app.MapHub<NotificationHub>("/notificationHub")
    .RequireCors(CorsConfiguration.AllowFrontendPolicy);

app.Run();

public partial class Program { }