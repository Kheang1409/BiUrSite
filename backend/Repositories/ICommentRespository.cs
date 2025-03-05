using Backend.Models;

namespace Backend.Repositories
{
    public interface ICommentRepository
    {
        Task<List<Comment>> GetCommentsAsync(int pageNumber, string keyword, int ? userId, int ? postId);
        Task<Comment?> GetCommentByIdAsync(int postId, int commentId);
        Task AddCommentAsync(Comment comment);
        Task<bool> UpdateContentAsync(int postId, int commentId, string description);
        Task<bool> SoftDeleteCommentAsync(int postId, int commentId);
        Task<bool> DeleteCommentAsync(int postId, int commentId);
    }
}
