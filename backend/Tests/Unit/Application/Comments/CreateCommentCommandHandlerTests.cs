using Backend.Application.Comments.Create;
using Backend.Application.Data;
using Backend.Domain.Comments;
using Backend.Domain.Posts;
using Backend.Domain.Users;
using Backend.Domain.Primitive;
using Backend.SharedKernel.Exceptions;
using FluentAssertions;
using Moq;
using Tests.TestFixtures;
using Xunit;

namespace Tests.Unit.Application.Comments;

public class CreateCommentCommandHandlerTests : TestBase
{
    private readonly Mock<IPostRepository> _postRepositoryMock;
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateCommentCommandHandler _handler;

    public CreateCommentCommandHandlerTests()
    {
        _postRepositoryMock = new Mock<IPostRepository>();
        _commentRepositoryMock = new Mock<ICommentRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CreateCommentCommandHandler(
            _postRepositoryMock.Object,
            _commentRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateComment()
    {
        var post = MockData.CreateFakePost();
        var userId = Guid.NewGuid();
        var command = new CreateCommentCommand(post.Id.Value, userId, "testuser", "Great post!");

        _postRepositoryMock
            .Setup(x => x.GetPostById(It.IsAny<PostId>()))
            .ReturnsAsync(post);

        _commentRepositoryMock
            .Setup(x => x.Create(It.IsAny<PostId>(), It.IsAny<Comment>()))
            .ReturnsAsync(It.IsAny<Comment>());

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Text.Should().Be("Great post!");
        _commentRepositoryMock.Verify(x => x.Create(It.IsAny<PostId>(), It.IsAny<Comment>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PostNotFound_ShouldThrowNotFoundException()
    {
        var command = new CreateCommentCommand(Guid.NewGuid(), Guid.NewGuid(), "user", "comment");

        _postRepositoryMock
            .Setup(x => x.GetPostById(It.IsAny<PostId>()))
            .ReturnsAsync((Post?)null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Post not found.");
    }
}
