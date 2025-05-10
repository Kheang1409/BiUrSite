using System.Security.Claims;
using System.Text;
using Backend.Api.Hubs;
using Backend.API.Middleware;
using Backend.Application.Behaviors;
using Backend.Application.Features.Auth.ForgotPassword;
using Backend.Application.Features.Auth.Login;
using Backend.Application.Features.Auth.ResetPassword;
using Backend.Application.Features.Auth.Verify;
using Backend.Application.Features.Comments.CreateComment;
using Backend.Application.Features.Comments.DeleteComment;
using Backend.Application.Features.Comments.GetCommentById;
using Backend.Application.Features.Comments.UpdateComment;
using Backend.Application.Features.Posts.CreatePost;
using Backend.Application.Features.Posts.DeletePost;
using Backend.Application.Features.Posts.GetPosts;
using Backend.Application.Features.Posts.UpdatePost;
using Backend.Application.Features.Users.BanUser;
using Backend.Application.Features.Users.RegisterOAuthUser;
using Backend.Application.Features.Users.RegisterUser;
using Backend.Infrastructure.Persistence;
using Bacnkend.Application.Services;
using Bacnkend.Infrastructure.Extensions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JWT"));
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY:") 
                    ?? builder.Configuration["JWT:SecretKey"]
                    ?? throw new InvalidOperationException("JWT:SecretKey is not configured.");
    var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
                    ?? builder.Configuration["JWT:Issuer"] 
                    ?? throw new InvalidOperationException("JWT:Issuer is not configured.");
    var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
                    ?? builder.Configuration["JWT:Audience"] 
                    ?? throw new InvalidOperationException("JWT:Audience is not configured.");
    var key = Encoding.UTF8.GetBytes(secretKey);

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = ClaimTypes.Role
    };
})
.AddCookie()
.AddGoogle(googleOptions =>
{
    var clientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") 
                    ?? builder.Configuration["Authentication:Google:ClientId"] 
                    ?? throw new InvalidOperationException("Authentication:Google:ClientId is not configured.");
    var clientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") 
                        ?? builder.Configuration["Authentication:Google:ClientSecret"] 
                        ?? throw new InvalidOperationException("Authentication:Google:ClientSecret is not configured.");

    googleOptions.ClientId = clientId;
    googleOptions.ClientSecret = clientSecret;
    googleOptions.CallbackPath = "/users/signin-google";
    googleOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddFacebook(facebookOptions =>
{

    var appId = Environment.GetEnvironmentVariable("FACEBOOK_APP_ID") 
                ?? builder.Configuration["Authentication:Facebook:AppId"] 
                ?? throw new InvalidOperationException("Authentication:Facebook:AppId is not configured.");
    var appSecret = Environment.GetEnvironmentVariable("FACEBOOK_APP_SECRET") 
                    ?? builder.Configuration["Authentication:Facebook:AppSecret"] 
                    ?? throw new InvalidOperationException("Authentication:Facebook:AppSecret is not configured.");
    
    facebookOptions.AppId = appId;
    facebookOptions.AppSecret = appSecret;
    facebookOptions.CallbackPath = "/signin-facebook";
    facebookOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
});;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>{
    c.SwaggerDoc("v1", new() { Title = "Backend API", Version = "v1" });

    // Add JWT Authentication
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token.\n\nExample: \"Bearer eyJhbGciOiJI...\""
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMediatR(
    typeof(LoginCommand).Assembly,
    typeof(RegisterUserCommand).Assembly, 
    typeof(RegisterOAuthUserCommand).Assembly, 
    typeof(VerifyCommand).Assembly,
    typeof(BanUserCommand).Assembly,
    typeof(ResetPasswordCommand).Assembly,
    typeof(ForgotPasswordCommand).Assembly,
    typeof(GetPostsCommand).Assembly,
    typeof(CreatePostCommand).Assembly,
    typeof(UpdatePostCommand).Assembly,
    typeof(DeletePostCommand).Assembly,
    typeof(CreateCommentCommand).Assembly,
    typeof(CreateCommentCommand).Assembly,
    typeof(UpdateCommentCommand).Assembly,
    typeof(DeleteCommentCommand).Assembly
);


builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterOAuthUserCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<VerifyCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<BanUserCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ResetPasswordCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ForgotPasswordCommandValidator>();

builder.Services.AddValidatorsFromAssemblyContaining<GetPostsCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreatePostWithUserIdCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdatePostWithPostIdCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<DeletePostCommandValidator>();

builder.Services.AddValidatorsFromAssemblyContaining<GetCommentsWithPostIdCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateCommentWithIdsCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateCommentWithIdCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<DeleteCommentCommandValidator>();

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

var allowedOrigins = Environment.GetEnvironmentVariable("AllowedOrigins")?.Split(";") 
                    ?? builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() 
                    ?? throw new InvalidOperationException("AllowedOrigins is not configured.");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", builder =>
    {
        builder.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
    });
});



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

    if (pendingMigrations.Any())
    {
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("Migrations applied successfully.");
    }
    else
    {
        Console.WriteLine("No pending migrations.");
    }
}

app.UseRouting();
app.UseCors("AllowSpecificOrigins");

app.UseAuthentication();
app.UseAuthorization(); 

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.MapHub<NotificationHub>("/notificationHub");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.Run();