using Backend.API.Middleware;
using Backend.Application;
using Backend.Infrastructure;
using Backend.Infrastructure.Authentication;
using Backend.Infrastructure.Configuration;
using Backend.Infrastructure.Hubs;
using Backend.Infrastructure.Swagger;
using Backend.Infrastructure.Configurations;

var builder = WebApplication.CreateBuilder(args);

StronglyTypedIdSerializationRegistry.Register();

builder.Services.AddControllers();
builder.Services.ConfigureAuthenticationServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddSignalR();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(CorsConfiguration.AllowFrontendPolicy);
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BiUrSite API v1");
    });
}
app.MapControllers();

app.MapHub<FeedHub>("/feedHub");
app.MapHub<NotificationHub>("/notificationHub");

app.Run();