using Backend.Models;

namespace Backend.Repositories
{
    public interface IPostRepository
    {
        Task<List<Post>> GetPostsAsync(int pageNumber, string? keyword);
        Task<Post?> GetPostByIdAsync(int postId);
        Task<int> GetUserTotalPostAsync(int userId);
        Task<Post> AddPostAsync(Post post);
        Task<bool> UpdateContentAsync(int postId, string description);
        Task<bool> SoftDeletePostAsync(int postId);
        Task<bool> DeletePostAsync(int postId);
    }
}
