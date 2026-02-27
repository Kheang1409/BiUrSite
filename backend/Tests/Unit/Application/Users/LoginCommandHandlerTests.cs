using Backend.Application.Services;
using Backend.Application.Users.Login;
using Backend.Domain.Users;
using Backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Tests.TestFixtures;
using Xunit;

namespace Tests.Unit.Application.Users;

public class LoginCommandHandlerTests : TestBase
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _handler = new LoginCommandHandler(_userRepositoryMock.Object, _passwordHasherMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ShouldReturnUser()
    {
        // Arrange
        var password = "ValidPassword123!";
        var user = new User.Builder()
            .SetUserName("testuser")
            .SetEmail("test@example.com")
            .SetPassword(password)
            .SetAuthProvider("local")
            .SetStatus(Status.Active)
            .SetRole(Role.User)
            .Build();

        var command = new LoginCommand("test@example.com", password);

        _userRepositoryMock
            .Setup(x => x.GetUserByEmail(command.Email))
            .ReturnsAsync(user);
        
        _passwordHasherMock
            .Setup(x => x.VerifyPassword(password, It.IsAny<string>()))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new LoginCommand("nonexistent@example.com", "password");

        _userRepositoryMock
            .Setup(x => x.GetUserByEmail(command.Email))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid username or password.");
    }

    [Fact]
    public async Task Handle_InvalidPassword_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var user = new User.Builder()
            .SetUserName("testuser")
            .SetEmail("test@example.com")
            .SetPassword("CorrectPassword123!")
            .SetAuthProvider("local")
            .SetStatus(Status.Active)
            .SetRole(Role.User)
            .Build();

        var command = new LoginCommand("test@example.com", "WrongPassword");

        _userRepositoryMock
            .Setup(x => x.GetUserByEmail(command.Email))
            .ReturnsAsync(user);
        
        _passwordHasherMock
            .Setup(x => x.VerifyPassword("WrongPassword", It.IsAny<string>()))
            .Returns(false);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid username or password.");
    }

    [Fact]
    public async Task Handle_UnverifiedUser_ShouldThrowUnauthorizedAccessException()
    {
        var originalTracing = Environment.GetEnvironmentVariable("ENABLE_REQUEST_TRACING");
        try
        {
            Environment.SetEnvironmentVariable("ENABLE_REQUEST_TRACING", "0");
            
            var password = "ValidPassword123!";
            var user = new User.Builder()
                .SetUserName("testuser")
                .SetEmail("test@example.com")
                .SetPassword(password)
                .SetAuthProvider("local")
                .SetStatus(Status.Unverified)
                .SetRole(Role.User)
                .Build();

            var command = new LoginCommand("test@example.com", password);

            _userRepositoryMock
                .Setup(x => x.GetUserByEmail(command.Email))
                .ReturnsAsync(user);

            var act = async () => await _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("User is Unverified.");
        }
        finally
        {
            Environment.SetEnvironmentVariable("ENABLE_REQUEST_TRACING", originalTracing);
        }
    }
}
