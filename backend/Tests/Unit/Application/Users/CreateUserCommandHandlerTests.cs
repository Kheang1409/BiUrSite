using Backend.Application.Users.Create;
using Backend.Application.Data;
using Backend.Domain.Users;
using Backend.Domain.Enums;
using Backend.Domain.Primitive;
using Backend.SharedKernel.Exceptions;
using FluentAssertions;
using Moq;
using Tests.TestFixtures;
using Xunit;

namespace Tests.Unit.Application.Users;

public class CreateUserCommandHandlerTests : TestBase
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUserFactory> _userFactoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _userFactoryMock = new Mock<IUserFactory>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        var factories = new List<IUserFactory> { _userFactoryMock.Object };
        _handler = new CreateUserCommandHandler(
            _userRepositoryMock.Object,
            factories,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_NewUser_ShouldCreateUser()
    {
        var command = new CreateUserCommand("newuser", "new@example.com", "password123");
        var userFactory = new UserFactory();

        var handler = new CreateUserCommandHandler(
            _userRepositoryMock.Object,
            new List<IUserFactory> { userFactory },
            _unitOfWorkMock.Object);

        _userRepositoryMock
            .Setup(x => x.GetUserByEmail(command.Email))
            .ReturnsAsync((User?)null);

        _userRepositoryMock
            .Setup(x => x.Create(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Email.Should().Be(command.Email);
        _userRepositoryMock.Verify(x => x.Create(It.IsAny<User>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingActiveUser_ShouldThrowConflictException()
    {
        var command = new CreateUserCommand("existinguser", "existing@example.com", "password123");
        var existingUser = new User.Builder()
            .SetUserName("existinguser")
            .SetEmail("existing@example.com")
            .SetPassword("password")
            .SetAuthProvider("local")
            .SetStatus(Status.Active)
            .SetRole(Role.User)
            .Build();

        _userRepositoryMock
            .Setup(x => x.GetUserByEmail(command.Email))
            .ReturnsAsync(existingUser);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Email is already registered.");
    }
}
