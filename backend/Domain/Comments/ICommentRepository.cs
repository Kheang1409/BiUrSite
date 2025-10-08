namespace Backend.Domain.Comments;

public interface ICommentRepository
{
    Task<IEnumerable<Comment>> GetComments(string postId, int pageNumber);
    Task<Comment?> GetCommentById(string postId, string commentId);
    Task<Comment> Create(string postId, Comment post);
    Task Update(string postId, Comment comment);
    Task Delete(string postId, Comment comment);
}