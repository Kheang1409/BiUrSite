using Backend.Domain.Comments.Entities;
using Backend.Domain.Common.Enums;
using Backend.Domain.Posts.Entities;
using Backend.Domain.Users.Entities;

namespace Backend.Infrastructure.Extensions;
public static class IQueryableExtensions
{
    public static IQueryable<Comment> FilterAvailableComments(this IQueryable<Comment> query)
    {
        return query
            .Where(comment => comment.Post != null && !comment.Post.IsDeleted)
            .Where(comment => !comment.IsDeleted) 
            .Where(comment => comment.Commenter != null && comment.Commenter.Status == Status.Active);
    }

    public static IQueryable<Post> FilterAvailablePost(this IQueryable<Post> query){
            return query
            .Where(post => !post.IsDeleted) 
            .Where(post => post.Author != null && post.Author.Status == Status.Active);
    }

    public static IQueryable<User> FilterUserByStatus(this IQueryable<User> query, Status status){
            return query
            .Where(user => user.Status == status);
    }
}