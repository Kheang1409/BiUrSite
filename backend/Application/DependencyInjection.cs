using Microsoft.Extensions.DependencyInjection;
using Backend.Application.Users.Create;
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
using Backend.Application.Posts.GetPost;
using Backend.Application.Posts.Create;
using Backend.Application.Posts.GetPosts;
using Backend.Application.Posts.Delete;
using Backend.Application.Comments.GetComments;
using Backend.Application.Comments.GetComment;
using Backend.Application.Comments.Create;
using Backend.Application.Comments.Edit;
using Backend.Application.Comments.Delete;

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

            cfg.RegisterServicesFromAssembly(typeof(UploadProfileImageHandler).Assembly);

            cfg.RegisterServicesFromAssembly(typeof(GetUserByIdQueryHandler).Assembly);

            cfg.RegisterServicesFromAssembly(typeof(VerifyUserCommand).Assembly);

            cfg.RegisterServicesFromAssembly(typeof(UpdateProfileCommandHandler).Assembly);

            cfg.RegisterServicesFromAssembly(typeof(GetPostByIdQueryHandler).Assembly);

            cfg.RegisterServicesFromAssembly(typeof(CreatePostCommandHandler).Assembly);

            cfg.RegisterServicesFromAssembly(typeof(GetPostsQueryHandler).Assembly);

            cfg.RegisterServicesFromAssembly(typeof(GetCommentsQueryHandler).Assembly);

            cfg.RegisterServicesFromAssembly(typeof(GetCommentByIdQueryHandler).Assembly);

            cfg.RegisterServicesFromAssembly(typeof(CreateCommentCommandHandler).Assembly);

            cfg.RegisterServicesFromAssembly(typeof(EditCommentCommandHandler).Assembly);

            cfg.RegisterServicesFromAssembly(typeof(DeleteCommentCommandHandler).Assembly);

        });

        services.AddValidatorsFromAssembly(typeof(CreateUserCommandHandler).Assembly);
        services.AddValidatorsFromAssembly(typeof(CreatePostCommandHandler).Assembly);
        services.AddTransient<IHandleMessages<UserCreatedEvent>, SendUserVerificationEmailHandler>();
        services.AddTransient<IHandleMessages<PostCreatedEvent>, UploadImageHandler>();
        services.AddTransient<IHandleMessages<PostCreatedEvent>, SendPostToFeedHandler>();
        services.AddTransient<IHandleMessages<UserForgotPasswordEvent>, SendUserOTPEmailHandler>();
        
        services.AddTransient<IHandleMessages<UpdatedProfileEvent>, UploadProfileImageHandler>();

        services.AddTransient<IHandleMessages<PostDeletedEvent>, DeleteImageHandler>();
        services.AddTransient<IHandleMessages<CommentCreatedEvent>, SendNotificationPostOwnerHandler>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        
        return services;
    }
}