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

public class UnbanUserCommandHandlerTests : TestBase
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly UnbanUserCommandHandler _handler;

    public UnbanUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();

        _handler = new UnbanUserCommandHandler(
            _userRepositoryMock.Object,
            NullLogger<UnbanUserCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUnbanUser()
    {
        var user = MockData.CreateFakeUser();
        user.Ban("test ban", null);
        var command = new UnbanUserCommand(user.Id.Value);

        _userRepositoryMock
            .Setup(x => x.GetUserById(It.IsAny<UserId>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(x => x.Update(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        await _handler.Handle(command, CancellationToken.None);

        user.Status.Should().Be(Status.Active);
        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowNotFoundException()
    {
        var command = new UnbanUserCommand(Guid.NewGuid());

        _userRepositoryMock
            .Setup(x => x.GetUserById(It.IsAny<UserId>()))
            .ReturnsAsync((User?)null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task Handle_NotBannedUser_ShouldThrowConflictException()
    {
        var user = MockData.CreateFakeUser();
        var command = new UnbanUserCommand(user.Id.Value);

        _userRepositoryMock
            .Setup(x => x.GetUserById(It.IsAny<UserId>()))
            .ReturnsAsync(user);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("User is not banned.");
    }
}
