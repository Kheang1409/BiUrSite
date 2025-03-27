using Backend.Models;
using Backend.Repositories;

namespace Backend.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;

        public PostService(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        public async Task<List<Post>> GetPostsAsync(int pageNumber, string? keyword)
            => await _postRepository.GetPostsAsync(pageNumber, keyword);

        public async Task<Post?> GetPostByIdAsync(int postId)
             => await _postRepository.GetPostByIdAsync(postId);

        public async Task<int> GetUserTotalPostAsync(int userId)
            => await _postRepository.GetUserTotalPostAsync(userId);
            
        public async Task<Post> AddPostAsync(Post post)
            => await _postRepository.AddPostAsync(post);

        public async Task<bool> UpdateContentAsync(int postId, string description)
            => await _postRepository.UpdateContentAsync(postId, description);

        public async Task<bool> SoftDeletePostAsync(int postId)
            => await _postRepository.SoftDeletePostAsync(postId);

        public async Task<bool> DeletePostAsync(int postId)
            => await _postRepository.DeletePostAsync(postId);
    }
}