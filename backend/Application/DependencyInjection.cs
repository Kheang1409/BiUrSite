using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;
using Rebus.Handlers;
using Backend.Application.Behavior;
using Backend.Application.Comments.Create;
using Backend.Application.Posts.Create;
using Backend.Application.Posts.Delete;
using Backend.Application.Users.Create;
using Backend.Application.Users.ForgotPassword;
using Backend.Application.Users.Update;

namespace Backend.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var applicationAssembly = typeof(DependencyInjection).Assembly;
        
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(applicationAssembly);
        });

        services.AddValidatorsFromAssembly(applicationAssembly);
        
        services.AddTransient<IHandleMessages<UserCreatedEvent>, SendUserVerificationEmailHandler>();
        services.AddTransient<IHandleMessages<PostCreatedEvent>, UploadImageHandler>();
        services.AddTransient<IHandleMessages<PostCreatedEvent>, SendPostToFeedHandler>();
        services.AddTransient<IHandleMessages<UserForgotPasswordEvent>, SendUserOTPEmailHandler>();
        services.AddTransient<IHandleMessages<UpdatedProfileEvent>, UploadProfileImageHandler>();
        services.AddTransient<IHandleMessages<PostDeletedEvent>, DeleteImageHandler>();
        services.AddTransient<IHandleMessages<CommentCreatedEvent>, SendNotificationPostOwnerHandler>();
        
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        
        return services;
    }
}