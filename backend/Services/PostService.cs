using AutoMapper;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;

        public PostService(IPostRepository postRepository, IMapper mapper)
        {
            _postRepository = postRepository;
            _mapper = mapper;
        }

        public async Task<List<PostDto>> GetPostsAsync(int pageNumber, string keyword, int ? userId){
            var posts = await _postRepository.GetPostsAsync(pageNumber, keyword, userId);
            return _mapper.Map<List<PostDto>>(posts);

        }

        public async Task<PostDto> GetPostByIdAsync(int postId){
             var post = await _postRepository.GetPostByIdAsync(postId);
             return _mapper.Map<PostDto>(post);
        }
            


        public async Task AddPostAsync(Post post)
            => await _postRepository.AddPostAsync(post);

        public async Task<bool> UpdateContentAsync(int postId, string description)
            => await _postRepository.UpdateContentAsync(postId, description);

        public async Task<bool> DeletePostAsync(int postId)
            => await _postRepository.DeletePostAsync(postId);
    }
}