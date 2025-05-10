using Backend.Domain.Comments.Entities;

namespace Backend.Domain.Comments.Interfaces;
public interface ICommentRepository
{
    Task<List<Comment>> GetCommentsAsync(int postId, int pageNumber);
    Task<Comment?> GetCommentByIdAsync(int commentId);
    Task<Comment> CreateCommentAsync(Comment comment);
    Task<bool> UpdateCommentAsync(int commentId, string description);
    Task<bool> SoftDeleteCommentAsync(int commentId);
    Task<bool> DeleteCommentAsync(int commentId);
}
