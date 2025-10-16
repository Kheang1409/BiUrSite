using Backend.Application.Posts.Edit;
using Backend.Domain.Posts;
using Backend.SharedKernel.Exceptions;
using FluentAssertions;
using Moq;
using Tests.TestFixtures;
using Xunit;

namespace Tests.Unit.Application.Posts;

public class EditPostCommandHandlerTests : TestBase
{
    private readonly Mock<IPostRepository> _postRepositoryMock;
    private readonly EditPostCommandHandler _handler;

    public EditPostCommandHandlerTests()
    {
        _postRepositoryMock = new Mock<IPostRepository>();
        _handler = new EditPostCommandHandler(_postRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdatePost()
    {
        var post = MockData.CreateFakePost();
        var command = new EditPostCommand(post.Id.Value, post.UserId.Value, "Updated content");

        _postRepositoryMock
            .Setup(x => x.GetPostById(It.IsAny<PostId>()))
            .ReturnsAsync(post);

        _postRepositoryMock
            .Setup(x => x.Update(It.IsAny<Post>()))
            .Returns(Task.CompletedTask);

        await _handler.Handle(command, CancellationToken.None);

        post.Text.Should().Be("Updated content");
        _postRepositoryMock.Verify(x => x.Update(It.IsAny<Post>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PostNotFound_ShouldThrowNotFoundException()
    {
        var command = new EditPostCommand(Guid.NewGuid(), Guid.NewGuid(), "Updated content");

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
        var command = new EditPostCommand(post.Id.Value, differentUserId, "Updated content");

        _postRepositoryMock
            .Setup(x => x.GetPostById(It.IsAny<PostId>()))
            .ReturnsAsync(post);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("You are not authorized to edit this post.");
    }
}
