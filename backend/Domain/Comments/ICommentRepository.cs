using Backend.Domain.Posts;

namespace Backend.Domain.Comments;

public interface ICommentRepository
{
    Task<IEnumerable<Comment>> GetComments(PostId postId, int pageNumber);
    Task<Comment?> GetCommentById(PostId postId, CommentId commentId);
    Task<Comment> Create(PostId postId, Comment post);
    Task Update(PostId postId, Comment comment);
    Task Delete(PostId postId, Comment comment);
}