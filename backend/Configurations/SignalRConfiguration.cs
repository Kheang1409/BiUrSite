using Backend.Hubs;

namespace Backend.Configurations
{
    public static class SignalRConfiguration
    {
        public static void ConfigureSignalR(IServiceCollection services)
        {
            // Add SignalR services
            services.AddSignalR();
        }

        public static void ConfigureCorsForSignalR(IServiceCollection services, string[] allowedOrigins)
        {
            // Configure CORS policy for SignalR
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins", builder =>
                {
                    builder.WithOrigins(allowedOrigins)
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials();
                });
            });
        }

        public static void UseSignalR(IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<NotificationHub>("/notificationHub")
                        .RequireCors("AllowSpecificOrigins");
            });
        }
    }
}