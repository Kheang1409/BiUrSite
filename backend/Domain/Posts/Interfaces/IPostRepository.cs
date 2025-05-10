
using Backend.Domain.Posts.Entities;

namespace Backend.Domain.Posts.Interfaces;
public interface IPostRepository
{
    Task<List<Post>> SearchPostsAsync(string? keyword, int pageNumber);
    Task<Post?> GetPostByIdAsync(int postId);
    Task<int> GetUserPostCountAsync(int userId);
    Task<Post> CreatePostAsync(Post post);
    Task<bool> UpdatePostDescriptionAsync(int postId, string description);
    Task<bool> SoftDeletePostAsync(int postId);
    Task<bool> DeletePostAsync(int postId);
}
