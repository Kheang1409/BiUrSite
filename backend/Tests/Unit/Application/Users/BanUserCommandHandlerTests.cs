using Backend.Application.Users.Admin;
using Backend.Domain.Enums;
using Backend.Domain.Users;
using Backend.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Tests.TestFixtures;
using Xunit;

namespace Tests.Unit.Application.Users;

public class BanUserCommandHandlerTests : TestBase
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly BanUserCommandHandler _handler;

    public BanUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();

        _handler = new BanUserCommandHandler(
            _userRepositoryMock.Object,
            NullLogger<BanUserCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldBanUser()
    {
        var user = MockData.CreateFakeUser();
        var command = new BanUserCommand(user.Id.Value, "Spam", 1440);

        _userRepositoryMock
            .Setup(x => x.GetUserById(It.IsAny<UserId>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(x => x.Update(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        await _handler.Handle(command, CancellationToken.None);

        user.Status.Should().Be(Status.Banned);
        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PermanentBan_ShouldBanWithNoDuration()
    {
        var user = MockData.CreateFakeUser();
        var command = new BanUserCommand(user.Id.Value, "Severe violation", null);

        _userRepositoryMock
            .Setup(x => x.GetUserById(It.IsAny<UserId>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(x => x.Update(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        await _handler.Handle(command, CancellationToken.None);

        user.Status.Should().Be(Status.Banned);
        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowNotFoundException()
    {
        var command = new BanUserCommand(Guid.NewGuid(), "Spam", null);

        _userRepositoryMock
            .Setup(x => x.GetUserById(It.IsAny<UserId>()))
            .ReturnsAsync((User?)null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task Handle_DeletedUser_ShouldThrowConflictException()
    {
        var user = MockData.CreateFakeUser();
        user.Delete();
        var command = new BanUserCommand(user.Id.Value, "Spam", null);

        _userRepositoryMock
            .Setup(x => x.GetUserById(It.IsAny<UserId>()))
            .ReturnsAsync(user);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Cannot ban a deleted user.");
    }

    [Fact]
    public async Task Handle_AlreadyBannedUser_ShouldThrowConflictException()
    {
        var user = MockData.CreateFakeUser();
        user.Ban("previous ban", null);
        var command = new BanUserCommand(user.Id.Value, "Spam again", null);

        _userRepositoryMock
            .Setup(x => x.GetUserById(It.IsAny<UserId>()))
            .ReturnsAsync(user);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("User is already banned.");
    }

    [Fact]
    public async Task Handle_AdminUser_ShouldThrowForbiddenException()
    {
        var adminUser = new User.Builder()
            .SetUserName("admin")
            .SetEmail("admin@example.com")
            .SetPassword("password")
            .SetAuthProvider("local")
            .SetStatus(Status.Active)
            .SetRole(Role.Admin)
            .Build();

        var command = new BanUserCommand(adminUser.Id.Value, "Attempt", null);

        _userRepositoryMock
            .Setup(x => x.GetUserById(It.IsAny<UserId>()))
            .ReturnsAsync(adminUser);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Cannot ban an admin user.");
    }
}
