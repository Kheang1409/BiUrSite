using Backend.Domain.Users;

namespace Backend.Domain.Posts;

public class PostFactory : IPostFactory
{
    public Post Create(UserId userId, string username, string text, byte[]? data)
    {
        var post = new Post.Builder()
                    .WithUserId(userId)
                    .WithUsername(username)
                    .WithText(text)
                    .WithImage(data)
                    .Build();
        return post;
    }
}
