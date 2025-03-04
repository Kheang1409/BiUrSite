using Backend.Models;
namespace Backend.Services
{
    public interface IPostService
    {

        Task<List<PostDto>> GetPostsAsync(int pageNumber, string keyword, int ? userId);
        Task<PostDto?> GetPostByIdAsync(int postId);
        Task AddPostAsync(Post post);
        Task<bool> UpdateContentAsync(int postId, string description);
        Task<bool> DeletePostAsync(int postId);
    }
}