using Backend.Domain.Users;

namespace Backend.Domain.Posts;

public interface IPostRepository
{
    Task<IEnumerable<Post>> GetPosts(UserId? userId, string? keywords, int pageNumber);
    Task<Post?> GetPostById(PostId id);
    Task<Post> Create(Post post);
    Task Update(Post post);
    Task Delete(Post post);
}