using Backend.Application.Comments.Create;
using Backend.Application.DTOs.Notifications;
using Backend.Domain.Posts;
using Backend.Infrastructure.Hubs;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Repositories;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Tests.TestFixtures;
using Backend.Infrastructure.Notifications;

namespace Tests.Integration;

[Collection("MongoDB")]
public class NotificationsIntegrationTests
{
    private readonly MongoDbFixture _fixture;
    private readonly UserRepository _userRepository;
    private readonly PostRepository _postRepository;
    private readonly CommentRepository _commentRepository;
    private readonly NotificationRepository _notificationRepository;

    public NotificationsIntegrationTests(MongoDbFixture fixture)
    {
        _fixture = fixture;
        _userRepository = new UserRepository(_fixture.Context, Mock.Of<ILogger<UserRepository>>());
        _postRepository = new PostRepository(_fixture.Context, Mock.Of<ILogger<PostRepository>>());
        _commentRepository = new CommentRepository(_fixture.Context, Mock.Of<ILogger<CommentRepository>>());
        _notificationRepository = new NotificationRepository(_fixture.Context, Mock.Of<ILogger<NotificationRepository>>());
    }

    [Fact]
    public async Task Notification_Flow_Persists_And_Sends_SignalR()
    {
        var owner = MockData.CreateFakeUser();
        var commenter = MockData.CreateFakeUser();

        await _fixture.Context.Users.InsertOneAsync(owner);
        await _fixture.Context.Users.InsertOneAsync(commenter);

        var post = new Post.Builder()
            .WithUserId(owner.Id)
            .WithUsername(owner.Username)
            .WithText("Hello world from owner")
            .WithUser(owner)
            .Build();

        await _postRepository.Create(post);

        var comment = post.AddComment(commenter, "Nice post!");
        await _commentRepository.Create(post.Id, comment);
        var mockHubContext = new Mock<IHubContext<NotificationHub>>();
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();

        mockClients.Setup(c => c.User(It.IsAny<string>())).Returns(mockClientProxy.Object);
        mockHubContext.SetupGet(h => h.Clients).Returns(mockClients.Object);

        var notifier = new NotificationNotifier(mockHubContext.Object, Mock.Of<ILogger<NotificationNotifier>>());

        var handler = new SendNotificationPostOwnerHandler(
            _userRepository,
            _postRepository,
            _commentRepository,
            _notificationRepository,
            notifier
        );

        var evt = new CommentCreatedEvent(post.Id.Value, comment.Id.Value, commenter.Id.Value);

        // Act: handle event (this should persist notification and call SignalR)
        await handler.Handle(evt);

        var ownerFromDb = await _fixture.Context.Users.Find(u => u.Id == owner.Id).FirstOrDefaultAsync();
        ownerFromDb.Should().NotBeNull();
        ownerFromDb!.HasNewNotification.Should().BeTrue();
        ownerFromDb.Notifications.Should().Contain(n => n.PostId.Value == post.Id.Value && n.Message == comment.Text);

        // Assert: SignalR was invoked with NotificationDTO
        mockClientProxy.Verify(
            p => p.SendCoreAsync(
                It.Is<string>(s => s == "ReceiveCommentNotification"),
                It.Is<object[]>(o => o.Length == 1 && o[0] != null && o[0].GetType() == typeof(NotificationDTO) && ((NotificationDTO)o[0]).Message == comment.Text),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
