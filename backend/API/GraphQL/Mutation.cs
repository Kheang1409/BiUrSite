using Backend.Application.Comments.Create;
using Backend.Application.Comments.Delete;
using Backend.Application.Comments.Edit;
using Backend.Application.DTOs.Comments;
using Backend.Application.DTOs.Posts;
using Backend.Application.DTOs.Users;
using Backend.Application.Posts.Create;
using Backend.Application.Posts.Delete;
using Backend.Application.Posts.Edit;
using Backend.Application.Services;
using Backend.Application.Users.Create;
using Backend.Application.Users.Delete;
using Backend.Application.Users.ForgotPassword;
using Backend.Application.Users.Login;
using Backend.Application.Users.ResetPassword;
using Backend.Application.Users.Update;
using Backend.Application.Users.UpdateProfileNotificationStatus;
using Backend.Application.Users.VerifyUser;
using HotChocolate.Authorization;
using MediatR;

namespace Backend.API.GraphQL;

public sealed class Mutation
{
    public async Task<UserDto> Register(
        CreateUserCommand input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var user = await mediator.Send(input, cancellationToken);
        return (UserDto)user;
    }

    public async Task<TokenPayload> Login(
        string email,
        string password,
        [Service] IMediator mediator,
        [Service] ITokenService tokenService,
        CancellationToken cancellationToken)
    {
        var user = await mediator.Send(new LoginCommand(email, password), cancellationToken);
        if (user is null)
            throw new UnauthorizedAccessException("Invalid email or password.");
        var token = tokenService.GenerateToken(user.Id.Value, user.Email, user.Username, user.Role.ToString());
        return new TokenPayload(token);
    }

    public async Task<bool> VerifyUser(
        string token,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new VerifyUserCommand(token), cancellationToken);
        return true;
    }

    public async Task<bool> ForgotPassword(
        string email,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new ForgotPasswordCommand(email), cancellationToken);
        return true;
    }

    public async Task<bool> ResetPassword(
        ResetPasswordCommand input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.Send(input, cancellationToken);
        return true;
    }

    [Authorize]
    public async Task<PostDetailDto> CreatePost(
        string text,
        byte[]? data,
        [Service] GraphQLUserContext userContext,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var post = await mediator.Send(
            new CreatePostCommand(userContext.GetRequiredUserId(), userContext.GetRequiredUsername(), text, data),
            cancellationToken);
        return (PostDetailDto)post;
    }

    [Authorize]
    public async Task<bool> EditPost(
        Guid id,
        string text,
        [Service] GraphQLUserContext userContext,
        [Service] IMediator mediator,
        CancellationToken cancellationToken,
        byte[]? data = null,
        bool removeImage = false)
    {
        await mediator.Send(new EditPostCommand(id, userContext.GetRequiredUserId(), text, data, removeImage), cancellationToken);
        return true;
    }

    [Authorize]
    public async Task<bool> DeletePost(
        Guid id,
        [Service] GraphQLUserContext userContext,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new DeletePostCommand(id, userContext.GetRequiredUserId()), cancellationToken);
        return true;
    }

    [Authorize]
    public async Task<CommentDto> CreateComment(
        Guid postId,
        string text,
        [Service] GraphQLUserContext userContext,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var comment = await mediator.Send(new CreateCommentCommand(postId, userContext.GetRequiredUserId(), text), cancellationToken);
        return (CommentDto)comment;
    }

    [Authorize]
    public async Task<bool> EditComment(
        Guid postId,
        Guid id,
        string text,
        [Service] GraphQLUserContext userContext,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new EditCommentCommand(postId, id, userContext.GetRequiredUserId(), text), cancellationToken);
        return true;
    }

    [Authorize]
    public async Task<bool> DeleteComment(
        Guid postId,
        Guid id,
        [Service] GraphQLUserContext userContext,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteCommentCommand(postId, id, userContext.GetRequiredUserId()), cancellationToken);
        return true;
    }

    [Authorize]
    public async Task<bool> UpdateMe(
        string username,
        string bio,
        [Service] GraphQLUserContext userContext,
        [Service] IMediator mediator,
        CancellationToken cancellationToken,
        byte[]? data = null,
        string? phone = null,
        bool removeImage = false)
    {
        await mediator.Send(
            new UpdateProfileCommand(userContext.GetRequiredEmail(), username, bio, data, phone, removeImage),
            cancellationToken);
        return true;
    }

    [Authorize]
    public async Task<bool> MarkNotificationsAsRead(
        [Service] GraphQLUserContext userContext,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateProfileNotificationStatusCommand(userContext.GetRequiredEmail()), cancellationToken);
        return true;
    }

    [Authorize]
    public async Task<bool> DeleteMe(
        [Service] GraphQLUserContext userContext,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteUserCommand(userContext.GetRequiredUserId()), cancellationToken);
        return true;
    }
}

public sealed record TokenPayload(string Token);
