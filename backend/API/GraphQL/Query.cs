using Backend.Application.Comments.GetComments;
using Backend.Application.DTOs.Comments;
using Backend.Application.DTOs.Notifications;
using Backend.Application.DTOs.Posts;
using Backend.Application.DTOs.Users;
using Backend.Application.Notifications.GetNotifications;
using Backend.Application.Posts.GetMyPosts;
using Backend.Application.Posts.GetPost;
using Backend.Application.Posts.GetPosts;
using Backend.Application.Users.GetUser;
using Backend.Application.Users.GetUsers;
using Backend.Domain.Posts;
using Backend.SharedKernel.Exceptions;
using HotChocolate.Authorization;
using MediatR;

namespace Backend.API.GraphQL;

public sealed class Query
{
    public bool Health() => true;

    public async Task<IReadOnlyList<PostDto>> Posts(
        string? keywords,
        int pageNumber,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var posts = await mediator.Send(new GetPostsQuery(keywords, pageNumber), cancellationToken);
        return posts.Select(p => (PostDto)p).ToList();
    }

    public async Task<PostDetailDto> Post(
        Guid id,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var post = await mediator.Send(new GetPostByIdQuery(id), cancellationToken);
        if (post is null)
            throw new NotFoundException("Post is not found.");
        return (PostDetailDto)post;
    }

    [Authorize]
    public async Task<IReadOnlyList<PostDto>> MyPosts(
        int pageNumber,
        [Service] GraphQLUserContext userContext,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var posts = await mediator.Send(new GetMyPostsQuery(userContext.GetRequiredUserId(), pageNumber), cancellationToken);
        return posts.Select(p => (PostDto)p).ToList();
    }

    public async Task<IReadOnlyList<UserDto>> Users(
        int pageNumber,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var users = await mediator.Send(new GetUsersQuery(pageNumber), cancellationToken);
        return users.Select(u => (UserDto)u).ToList();
    }

    public async Task<UserDto> User(
        Guid id,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var user = await mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        if (user is null)
            throw new NotFoundException("User is not found.");
        return (UserDto)user;
    }

    [Authorize]
    public async Task<UserDto> Me(
        [Service] GraphQLUserContext userContext,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var user = await mediator.Send(new GetUserByIdQuery(userContext.GetRequiredUserId()), cancellationToken);
        if (user is null)
            throw new NotFoundException("User is not found.");
        return (UserDto)user;
    }

    [Authorize]
    public async Task<IReadOnlyList<NotificationDTO>> Notifications(
        int pageNumber,
        [Service] GraphQLUserContext userContext,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var notifications = await mediator.Send(new GetNotificationsQuery(userContext.GetRequiredUserId(), pageNumber), cancellationToken);
        return notifications.Select(n => (NotificationDTO)n).ToList();
    }

    public async Task<IReadOnlyList<CommentDto>> Comments(
        Guid postId,
        int pageNumber,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var comments = await mediator.Send(new GetCommentsQuery(postId, pageNumber), cancellationToken);
        return comments.Select(c => (CommentDto)c).ToList();
    }
}
