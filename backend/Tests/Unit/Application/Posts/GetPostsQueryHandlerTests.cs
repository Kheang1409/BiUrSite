using Backend.Application.Posts.GetPosts;
using Backend.Domain.Posts;
using FluentAssertions;
using Moq;
using Tests.TestFixtures;
using Xunit;

namespace Tests.Unit.Application.Posts;

public class GetPostsQueryHandlerTests : TestBase
{
    private readonly Mock<IPostRepository> _postRepositoryMock;
    private readonly GetPostsQueryHandler _handler;

    public GetPostsQueryHandlerTests()
    {
        _postRepositoryMock = new Mock<IPostRepository>();
        _handler = new GetPostsQueryHandler(_postRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithoutKeywords_ShouldReturnAllPosts()
    {
        var query = new GetPostsQuery(null, 1);
        var posts = MockData.CreateFakePosts(5);

        _postRepositoryMock
            .Setup(x => x.GetPosts(null, null, 1))
            .ReturnsAsync(posts);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task Handle_WithKeywords_ShouldReturnFilteredPosts()
    {
        var query = new GetPostsQuery("test", 1);
        var posts = MockData.CreateFakePosts(3);

        _postRepositoryMock
            .Setup(x => x.GetPosts(null, "test", 1))
            .ReturnsAsync(posts);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }
}
