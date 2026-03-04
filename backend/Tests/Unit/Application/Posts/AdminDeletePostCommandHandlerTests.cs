using Backend.Application.Posts.Admin;
using Backend.Application.Data;
using Backend.Domain.Enums;
using Backend.Domain.Posts;
using Backend.Domain.Primitive;
using Backend.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Tests.TestFixtures;
using Xunit;

namespace Tests.Unit.Application.Posts;

public class AdminDeletePostCommandHandlerTests : TestBase
{
    private readonly Mock<IPostRepository> _postRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AdminDeletePostCommandHandler _handler;

    public AdminDeletePostCommandHandlerTests()
    {
        _postRepositoryMock = new Mock<IPostRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new AdminDeletePostCommandHandler(
            _postRepositoryMock.Object,
            _unitOfWorkMock.Object,
            NullLogger<AdminDeletePostCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldDeletePost()
    {
        var post = MockData.CreateFakePost();
        var command = new AdminDeletePostCommand(post.Id.Value, "Violates community guidelines");

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
        var command = new AdminDeletePostCommand(Guid.NewGuid(), "Reason");

        _postRepositoryMock
            .Setup(x => x.GetPostById(It.IsAny<PostId>()))
            .ReturnsAsync((Post?)null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Post not found.");
    }

    [Fact]
    public async Task Handle_AlreadyDeletedPost_ShouldThrowConflictException()
    {
        var post = MockData.CreateFakePost();
        post.Delete();
        var command = new AdminDeletePostCommand(post.Id.Value, "Already gone");

        _postRepositoryMock
            .Setup(x => x.GetPostById(It.IsAny<PostId>()))
            .ReturnsAsync(post);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Post is already deleted.");
    }

    [Fact]
    public async Task Handle_NoReason_ShouldStillDeletePost()
    {
        var post = MockData.CreateFakePost();
        var command = new AdminDeletePostCommand(post.Id.Value, null);

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
    }
}
