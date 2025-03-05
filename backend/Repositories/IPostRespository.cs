using Backend.Models;

namespace Backend.Repositories
{
    public interface IPostRepository
    {
        Task<List<Post>> GetPostsAsync(int pageNumber, string keyword, int ? userId);
        Task<Post?> GetPostByIdAsync(int postId);
        Task AddPostAsync(Post post);
        Task<bool> UpdateContentAsync(int postId, string description);
        Task<bool> SoftDeletePostAsync(int postId);
        Task<bool> DeletePostAsync(int postId);
    }
}
