using Backend.Domain.Users;
using Backend.Domain.Enums;
using Backend.Domain.Images;
using Backend.Domain.Posts;
using FluentAssertions;
using Tests.TestFixtures;
using Xunit;
using Backend.Domain.Notifications;

namespace Tests.Unit.Domain;

public class UserTests : TestBase
{
    [Fact]
    public void CreateUser_WithValidData_ShouldCreateSuccessfully()
    {
        var username = "testuser";
        var email = "test@example.com";
        var password = "password123";

        var user = new User.Builder()
            .SetUserName(username)
            .SetEmail(email)
            .SetPassword(password)
            .SetAuthProvider("local")
            .SetStatus(Status.Active)
            .SetRole(Role.User)
            .Build();

        user.Should().NotBeNull();
        user.Id.Value.Should().NotBe(Guid.Empty);
        user.Username.Should().Be(username);
        user.Email.Should().Be(email);
        user.Status.Should().Be(Status.Active);
        user.Role.Should().Be(Role.User);
        user.Password.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CreateUser_WithOAuth_ShouldNotRequirePassword()
    {
        var user = new User.Builder()
            .SetUserName("oauthuser")
            .SetEmail("oauth@example.com")
            .SetAuthProvider("google")
            .SetStatus(Status.Active)
            .SetRole(Role.User)
            .Build();

        user.Should().NotBeNull();
        user.AuthProvider.Should().Be("google");
    }

    [Fact]
    public void CreateUser_WithoutUsername_ShouldThrowException()
    {
        var act = () => new User.Builder()
            .SetEmail("test@example.com")
            .SetPassword("password")
            .SetAuthProvider("local")
            .Build();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Username cannot be empty.");
    }

    [Fact]
    public void CreateUser_WithoutEmail_ShouldThrowException()
    {
        var act = () => new User.Builder()
            .SetUserName("testuser")
            .SetPassword("password")
            .SetAuthProvider("local")
            .Build();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Email cannot be empty.");
    }

    [Fact]
    public void User_ShouldHaveUniqueId()
    {
        var user1 = MockData.CreateFakeUser();
        var user2 = MockData.CreateFakeUser();

        user1.Id.Should().NotBe(user2.Id);
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateProfile()
    {
        var user = MockData.CreateFakeUser();
        var newUsername = "UpdatedUser";
        var newBio = "Updated bio";
        var newProfile = new Image("https://example.com/new.jpg");

        user.Update(newUsername, newBio, newProfile);

        user.Username.Should().Be(newUsername);
        user.Bio.Should().Be(newBio);
        user.Profile.Url.Should().Be("https://example.com/new.jpg");
        user.ModifiedDate.Should().NotBeNull();
    }

    [Fact]
    public void Verify_ShouldSetStatusToActive()
    {
        // Create a user that is not yet verified (Unverified status with token)
        var user = new User.Builder()
            .SetUserName("pendinguser")
            .SetEmail("pending@example.com")
            .SetPassword("password123")
            .SetAuthProvider("local")
            .SetStatus(Status.Unverified)
            .SetRole(Role.User)
            .SetToken(Token.Generate())
            .Build();

        user.Verify();

        user.Status.Should().Be(Status.Active);
        user.Token.Should().BeNull();
    }

    [Fact]
    public void Delete_ShouldSetStatusToDeleted()
    {
        var user = MockData.CreateFakeUser();

        user.Delete();

        user.Status.Should().Be(Status.Deleted);
        user.DeletedDate.Should().NotBeNull();
    }

    [Fact]
    public void ResetPassword_ShouldUpdatePassword()
    {
        var user = MockData.CreateFakeUser();
        var newPassword = "newPassword123";
        var oldPassword = user.Password;

        user.ResetPassword(newPassword);

        user.Password.Should().NotBe(oldPassword);
        user.Password.Should().NotBeNullOrEmpty();
        user.Otp.Should().BeNull();
    }

    [Fact]
    public void ForgotPassword_ShouldGenerateOtp()
    {
        var user = MockData.CreateFakeUser();

        user.ForgotPassword();

        user.Otp.Should().NotBeNull();
        user.Otp!.Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void MarkNotificationsAsRead_ShouldSetHasNewNotificationToFalse()
    {
        var user = MockData.CreateFakeUser();

        user.MarkNotificationsAsRead();

        user.HasNewNotification.Should().BeFalse();
    }

    [Fact]
    public void AddNotification_ShouldAddNotificationToList()
    {
        var user = MockData.CreateFakeUser();
        var postId = new PostId(Guid.NewGuid());
        var notification = Notification.Create(
                user.Id,
                postId,
                "commented on your post"
            );

        user.AddNotification(notification);
        user.Notifications.Should().Contain(notification);
        notification.UserId.Should().Be(user.Id);
    }

    [Fact]
    public void Verify_WhenAlreadyActive_ShouldThrow()
    {
        // Create an already active user
        var user = MockData.CreateFakeUser();
        // MockData.CreateFakeUser() creates Active users, so calling Verify should throw

        var act = () => user.Verify();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("User is already verified.");
    }

    [Fact]
    public void Verify_WhenDeleted_ShouldThrow()
    {
        var user = MockData.CreateFakeUser();
        user.Delete();

        var act = () => user.Verify();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot verify a deleted user.");
    }

    [Fact]
    public void Delete_WhenAlreadyDeleted_ShouldBeIdempotent()
    {
        var user = MockData.CreateFakeUser();
        user.Delete();
        var firstDeletedDate = user.DeletedDate;

        // Second delete should be a no-op
        user.Delete();

        user.Status.Should().Be(Status.Deleted);
        user.DeletedDate.Should().Be(firstDeletedDate);
    }
}
