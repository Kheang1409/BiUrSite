using Backend.Application.Posts.Delete;
using Backend.Application.Data;
using Backend.Domain.Posts;
using Backend.Domain.Primitive;
using Backend.SharedKernel.Exceptions;
using FluentAssertions;
using Moq;
using Tests.TestFixtures;
using Xunit;

namespace Tests.Unit.Application.Posts;

public class DeletePostCommandHandlerTests : TestBase
{
    private readonly Mock<IPostRepository> _postRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeletePostCommandHandler _handler;

    public DeletePostCommandHandlerTests()
    {
        _postRepositoryMock = new Mock<IPostRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new DeletePostCommandHandler(
            _postRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldDeletePost()
    {
        var post = MockData.CreateFakePost();
        var command = new DeletePostCommand(post.Id.Value, post.UserId.Value);

        _postRepositoryMock
            .Setup(x => x.GetPostById(It.IsAny<PostId>()))
            .ReturnsAsync(post);

        _postRepositoryMock
            .Setup(x => x.Delete(It.IsAny<Post>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _handler.Handle(command, CancellationToken.None);

        _postRepositoryMock.Verify(x => x.Delete(It.IsAny<Post>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PostNotFound_ShouldThrowNotFoundException()
    {
        var command = new DeletePostCommand(Guid.NewGuid(), Guid.NewGuid());

        _postRepositoryMock
            .Setup(x => x.GetPostById(It.IsAny<PostId>()))
            .ReturnsAsync((Post?)null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Post is not found.");
    }

    [Fact]
    public async Task Handle_UnauthorizedUser_ShouldThrowForbiddenException()
    {
        var post = MockData.CreateFakePost();
        var differentUserId = Guid.NewGuid();
        var command = new DeletePostCommand(post.Id.Value, differentUserId);

        _postRepositoryMock
            .Setup(x => x.GetPostById(It.IsAny<PostId>()))
            .ReturnsAsync(post);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("You are not authorized to edit this post.");
    }
}
