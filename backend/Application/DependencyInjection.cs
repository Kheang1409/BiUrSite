using Microsoft.Extensions.DependencyInjection;
using Backend.Application.Users.CreateUser;
using Backend.Application.Users.CreateByOAuth;
using FluentValidation;
using MediatR;
using Backend.Application.Behavior;
using Backend.Application.Users.Login;
using Backend.Application.Users.GetUser;
using Rebus.Handlers;
using Backend.Application.Users.VerifyUser;
using Backend.Application.Users.ForgotPassword;
using Backend.Application.Users.Update;

namespace Backend.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateUserCommandHandler).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(SendUserVerificationEmailHandler).Assembly);

            cfg.RegisterServicesFromAssembly(typeof(CreateUserByOAuthCommandHandler).Assembly);

            cfg.RegisterServicesFromAssembly(typeof(LoginCommandHandler).Assembly);

            cfg.RegisterServicesFromAssembly(typeof(ForgotPasswordCommandHandler).Assembly);

            cfg.RegisterServicesFromAssembly(typeof(ForgotPasswordCommandHandler).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(SendUserOTPEmailHandler).Assembly);


            cfg.RegisterServicesFromAssembly(typeof(GetUserByIdQueryHandler).Assembly);

            cfg.RegisterServicesFromAssembly(typeof(VerifyUserCommand).Assembly);

            cfg.RegisterServicesFromAssembly(typeof(UpdateProfileCommandHandler).Assembly);
        });

        services.AddValidatorsFromAssembly(typeof(CreateUserCommandHandler).Assembly);
        services.AddTransient<IHandleMessages<UserCreatedEvent>, SendUserVerificationEmailHandler>();
        services.AddTransient<IHandleMessages<UserForgotPasswordEvent>, SendUserOTPEmailHandler>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }
}