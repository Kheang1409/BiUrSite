using Backend.Domain.Users;

namespace Backend.Domain.Posts;

public interface IPostFactory
{
    Post Create(UserId userId, string username, string text, byte[]? data);
}
