using Backend.Application.Users.GetUser;
using Backend.Domain.Users;
using FluentAssertions;
using Moq;
using Tests.TestFixtures;
using Xunit;

namespace Tests.Unit.Application.Users;

public class GetUserByIdQueryHandlerTests : TestBase
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_UserExists_ShouldReturnUser()
    {
        var user = MockData.CreateFakeUser();
        var query = new GetUserByIdQuery(user.Id.Value);

        _userRepositoryMock
            .Setup(x => x.GetUserById(It.IsAny<UserId>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().Be(user);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnNull()
    {
        var query = new GetUserByIdQuery(Guid.NewGuid());

        _userRepositoryMock
            .Setup(x => x.GetUserById(It.IsAny<UserId>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }
}
