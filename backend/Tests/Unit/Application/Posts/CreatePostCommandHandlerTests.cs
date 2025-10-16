using Backend.Application.Posts.Create;
using Backend.Application.Data;
using Backend.Domain.Posts;
using Backend.Domain.Users;
using Backend.Domain.Primitive;
using FluentAssertions;
using Moq;
using Tests.TestFixtures;
using Xunit;

namespace Tests.Unit.Application.Posts;

public class CreatePostCommandHandlerTests : TestBase
{
    private readonly Mock<IPostRepository> _postRepositoryMock;
    private readonly Mock<IPostFactory> _postFactoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreatePostCommandHandler _handler;

    public CreatePostCommandHandlerTests()
    {
        _postRepositoryMock = new Mock<IPostRepository>();
        _postFactoryMock = new Mock<IPostFactory>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CreatePostCommandHandler(
            _postRepositoryMock.Object,
            _postFactoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreatePost()
    {
        var userId = Guid.NewGuid();
        var command = new CreatePostCommand(userId, "testuser", "Test post content", null);
        var post = MockData.CreateFakePost();

        _postFactoryMock
            .Setup(x => x.Create(It.IsAny<UserId>(), command.Username, command.Text, command.Data))
            .Returns(post);

        _postRepositoryMock
            .Setup(x => x.Create(It.IsAny<Post>()))
            .ReturnsAsync(post);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        _postRepositoryMock.Verify(x => x.Create(It.IsAny<Post>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
