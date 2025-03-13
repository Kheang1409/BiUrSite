using Backend.Enums;
using Backend.Models;

namespace Backend.Extensions{
    public static class IQueryableExtensions
    {
        public static IQueryable<Comment> FilterAvailableComments(this IQueryable<Comment> query)
        {
            return query
                .Where(comment => comment.post != null && !comment.post.isDeleted)
                .Where(comment => !comment.isDeleted) 
                .Where(comment => comment.commenter != null && comment.commenter.status == Status.Verified);
        }

        public static IQueryable<Post> FilterAvailablePost(this IQueryable<Post> query){
             return query
                .Where(post => !post.isDeleted) 
                .Where(post => post.author != null && post.author.status == Status.Verified);
        }

        public static IQueryable<User> FilterUserByStatus(this IQueryable<User> query, Status status){
             return query
                .Where(user => user.status == status);
        }
    }
}