using Backend.API.Middleware;
using Backend.Infrastructure.Configuration;
using Backend.Infrastructure.Hubs;
using Microsoft.AspNetCore.Builder;

namespace Backend.API.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseDiagnostics(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestDiagnosticMiddleware>();
    }

    public static WebApplication MapEndpoints(this WebApplication app)
    {
        app.MapGraphQL("/graphql")
            .RequireCors(CorsConfiguration.AllowFrontendPolicy);

        app.MapHub<FeedHub>("/feedHub")
            .RequireCors(CorsConfiguration.AllowFrontendPolicy);
        
        app.MapHub<NotificationHub>("/notificationHub")
            .RequireCors(CorsConfiguration.AllowFrontendPolicy);

        return app;
    }
}
