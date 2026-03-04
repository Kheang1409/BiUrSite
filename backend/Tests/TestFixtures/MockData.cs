using Bogus;
using Backend.Domain.Users;
using Backend.Domain.Posts;
using Backend.Domain.Comments;
using Backend.Domain.Enums;

namespace Tests.TestFixtures;

public static class MockData
{
    private static readonly Faker _faker = new();

    public static User CreateFakeUser()
    {
        return new User.Builder()
            .SetUserName(_faker.Internet.UserName())
            .SetEmail(_faker.Internet.Email())
            .SetPassword(_faker.Internet.Password())
            .SetAuthProvider("local")
            .SetStatus(Status.Active)
            .SetRole(Role.User)
            .Build();
    }

    public static Post CreateFakePost()
    {
        var user = CreateFakeUser();
        return new Post.Builder()
            .WithUserId(user.Id)
            .WithUsername(user.Username)
            .WithUser(user)
            .WithText(_faker.Lorem.Sentence())
            .Build();
    }

    public static List<User> CreateFakeUsers(int count = 10)
    {
        return Enumerable.Range(0, count)
            .Select(_ => CreateFakeUser())
            .ToList();
    }

    public static List<Post> CreateFakePosts(int count = 10)
    {
        return Enumerable.Range(0, count)
            .Select(_ => CreateFakePost())
            .ToList();
    }

    public static string CreateFakeEmail() => _faker.Internet.Email();
    public static string CreateFakeUsername() => _faker.Internet.UserName();
    public static string CreateFakePassword() => _faker.Internet.Password();
    public static string CreateFakeBio() => _faker.Lorem.Sentence();
    public static Guid CreateFakeGuid() => Guid.NewGuid();

    public static Comment CreateFakeComment(User? user = null)
    {
        var commentUser = user ?? CreateFakeUser();
        return Comment.Create(commentUser, _faker.Lorem.Sentence());
    }

    public static User CreateFakeAdminUser()
    {
        return new User.Builder()
            .SetUserName(_faker.Internet.UserName())
            .SetEmail(_faker.Internet.Email())
            .SetPassword(_faker.Internet.Password())
            .SetAuthProvider("local")
            .SetStatus(Status.Active)
            .SetRole(Role.Admin)
            .Build();
    }
}
