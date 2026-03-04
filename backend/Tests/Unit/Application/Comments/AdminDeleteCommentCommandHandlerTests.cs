using Backend.Application.Comments.Admin;
using Backend.Domain.Comments;
using Backend.Domain.Enums;
using Backend.Domain.Posts;
using Backend.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Tests.TestFixtures;
using Xunit;

namespace Tests.Unit.Application.Comments;

public class AdminDeleteCommentCommandHandlerTests : TestBase
{
    private readonly Mock<IPostRepository> _postRepositoryMock;
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
    private readonly AdminDeleteCommentCommandHandler _handler;

    public AdminDeleteCommentCommandHandlerTests()
    {
        _postRepositoryMock = new Mock<IPostRepository>();
        _commentRepositoryMock = new Mock<ICommentRepository>();

        _handler = new AdminDeleteCommentCommandHandler(
            _postRepositoryMock.Object,
            _commentRepositoryMock.Object,
            NullLogger<AdminDeleteCommentCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldDeleteComment()
    {
        var post = MockData.CreateFakePost();
        var user = MockData.CreateFakeUser();
        var comment = Comment.Create(user, "Test comment");

        var command = new AdminDeleteCommentCommand(post.Id.Value, comment.Id.Value, "Inappropriate");

        _postRepositoryMock
            .Setup(x => x.GetPostById(It.IsAny<PostId>()))
            .ReturnsAsync(post);

        _commentRepositoryMock
            .Setup(x => x.GetCommentById(It.IsAny<PostId>(), It.IsAny<CommentId>()))
            .ReturnsAsync(comment);

        _commentRepositoryMock
            .Setup(x => x.Delete(It.IsAny<PostId>(), It.IsAny<Comment>()))
            .Returns(Task.CompletedTask);

        await _handler.Handle(command, CancellationToken.None);

        _commentRepositoryMock.Verify(x => x.Delete(It.IsAny<PostId>(), It.IsAny<Comment>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PostNotFound_ShouldThrowNotFoundException()
    {
        var command = new AdminDeleteCommentCommand(Guid.NewGuid(), Guid.NewGuid(), "Reason");

        _postRepositoryMock
            .Setup(x => x.GetPostById(It.IsAny<PostId>()))
            .ReturnsAsync((Post?)null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Post not found.");
    }

    [Fact]
    public async Task Handle_InactivePost_ShouldThrowConflictException()
    {
        var post = MockData.CreateFakePost();
        post.Delete();
        var command = new AdminDeleteCommentCommand(post.Id.Value, Guid.NewGuid(), "Reason");

        _postRepositoryMock
            .Setup(x => x.GetPostById(It.IsAny<PostId>()))
            .ReturnsAsync(post);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Cannot delete comment on an inactive post.");
    }

    [Fact]
    public async Task Handle_CommentNotFound_ShouldThrowNotFoundException()
    {
        var post = MockData.CreateFakePost();
        var command = new AdminDeleteCommentCommand(post.Id.Value, Guid.NewGuid(), "Reason");

        _postRepositoryMock
            .Setup(x => x.GetPostById(It.IsAny<PostId>()))
            .ReturnsAsync(post);

        _commentRepositoryMock
            .Setup(x => x.GetCommentById(It.IsAny<PostId>(), It.IsAny<CommentId>()))
            .ReturnsAsync((Comment?)null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Comment not found.");
    }

    [Fact]
    public async Task Handle_AlreadyDeletedComment_ShouldThrowConflictException()
    {
        var post = MockData.CreateFakePost();
        var user = MockData.CreateFakeUser();
        var comment = Comment.Create(user, "Test comment");
        comment.Delete();

        var command = new AdminDeleteCommentCommand(post.Id.Value, comment.Id.Value, "Reason");

        _postRepositoryMock
            .Setup(x => x.GetPostById(It.IsAny<PostId>()))
            .ReturnsAsync(post);

        _commentRepositoryMock
            .Setup(x => x.GetCommentById(It.IsAny<PostId>(), It.IsAny<CommentId>()))
            .ReturnsAsync(comment);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Comment is already deleted.");
    }
}
