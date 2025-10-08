namespace Backend.Domain.Posts;

public interface IPostRepository
{
    Task<IEnumerable<Post>> GetPosts(string? username, string? keywords, int pageNumber);
    Task<Post?> GetPostById(string id);
    Task<Post> Create(Post post);
    Task Update(Post post);
    Task Delete(Post post);
}