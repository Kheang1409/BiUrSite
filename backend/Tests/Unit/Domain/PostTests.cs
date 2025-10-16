using Backend.Domain.Posts;
using Backend.Domain.Users;
using Backend.Domain.Enums;
using Backend.Domain.Comments;
using FluentAssertions;
using Tests.TestFixtures;
using Xunit;

namespace Tests.Unit.Domain;

public class PostTests : TestBase
{
    [Fact]
    public void CreatePost_WithValidData_ShouldCreateSuccessfully()
    {
        var userId = new UserId(Guid.NewGuid());
        var username = "testuser";
        var text = "Test post content";

        var post = new Post.Builder()
            .WithUserId(userId)
            .WithUsername(username)
            .WithText(text)
            .Build();

        post.Should().NotBeNull();
        post.Id.Value.Should().NotBe(Guid.Empty);
        post.UserId.Should().Be(userId);
        post.Username.Should().Be(username);
        post.Text.Should().Be(text);
        post.Status.Should().Be(Status.Active);
    }

    [Fact]
    public void CreatePost_WithoutUserId_ShouldThrowException()
    {
        var act = () => new Post.Builder()
            .WithUsername("testuser")
            .WithText("content")
            .Build();

        act.Should().Throw<ArgumentException>()
            .WithMessage("*UserId*");
    }

    [Fact]
    public void CreatePost_WithoutUsername_ShouldThrowException()
    {
        var act = () => new Post.Builder()
            .WithUserId(new UserId(Guid.NewGuid()))
            .WithText("content")
            .Build();

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Username*");
    }

    [Fact]
    public void CreatePost_WithoutText_ShouldThrowException()
    {
        var act = () => new Post.Builder()
            .WithUserId(new UserId(Guid.NewGuid()))
            .WithUsername("testuser")
            .Build();

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Text*");
    }

    [Fact]
    public void SetImage_ShouldUpdatePostImage()
    {
        var post = MockData.CreateFakePost();
        var imageUrl = "https://example.com/image.jpg";

        post.SetImage(imageUrl);

        post.Image.Should().NotBeNull();
        post.Image!.Url.Should().Be(imageUrl);
        post.ModifiedDate.Should().NotBeNull();
    }

    [Fact]
    public void UpdateContent_ShouldUpdateTextAndModifiedDate()
    {
        var post = MockData.CreateFakePost();
        var newContent = "Updated content";

        post.UpdateContent(newContent);

        post.Text.Should().Be(newContent);
        post.ModifiedDate.Should().NotBeNull();
    }

    [Fact]
    public void Delete_ShouldSetStatusToDeleted()
    {
        var post = MockData.CreateFakePost();

        post.Delete();

        post.Status.Should().Be(Status.Deleted);
        post.DeletedDate.Should().NotBeNull();
    }

    [Fact]
    public void AddComment_ShouldAddCommentToPost()
    {
        var post = MockData.CreateFakePost();
        var userId = new UserId(Guid.NewGuid());
        var username = "commenter";
        var text = "Nice post!";

        var comment = post.AddComment(userId, username, text);

        comment.Should().NotBeNull();
        comment.UserId.Should().Be(userId);
        comment.Text.Should().Be(text);
        post.Comments.Should().Contain(comment);
    }

    [Fact]
    public void Post_ShouldHaveUniqueId()
    {
        var post1 = MockData.CreateFakePost();
        var post2 = MockData.CreateFakePost();

        post1.Id.Should().NotBe(post2.Id);
    }
}
