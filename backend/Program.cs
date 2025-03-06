using Backend.Repositories;
using Backend.Services;
using Backend.Configurations;
using Backend.Middllewares;
using Backend.AutoMapper;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddSingleton<IRedisService, RedisService>();

builder.Services.AddScoped<IDatabaseService, DatabaseService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddAutoMapper(typeof(Program), typeof(UserProfile), typeof(PostProfile), typeof(CommentProfile));

// Configure redis
var redisService = builder.Services.BuildServiceProvider().GetRequiredService<IRedisService>();
redisService.ConfigureRedis(builder.Services, builder.Configuration);

// Configure authentication
var authenticationService = builder.Services.BuildServiceProvider().GetRequiredService<IAuthenticationService>();
authenticationService.ConfigureAuthentication(builder.Services, builder.Configuration);

// Configure database
var databaseService = builder.Services.BuildServiceProvider().GetRequiredService<IDatabaseService>();
databaseService.ConfigureDatabase(builder.Services, builder.Configuration);

// Configure email
var emailService = builder.Services.BuildServiceProvider().GetRequiredService<IEmailService>();
emailService.ConfigureEmailSettings(builder.Services, builder.Configuration);

// Add controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureSwaggerGen();

var app = builder.Build();

// Apply database migrations
databaseService.ApplyDatabaseMigrations(app);

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseMiddleware<CustomExceptionHandlerMiddleware>();

// Apply CORS policy
app.UseCors("AllowSpecificOrigin");

// Enable authorization and HTTPS redirection
app.UseAuthorization();
app.UseHttpsRedirection();  // This ensures the app will redirect HTTP to HTTPS

// Map controllers
app.MapControllers();

// Run the application
app.Run();