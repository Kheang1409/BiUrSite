using Backend.Application.Comments.Create;
using Backend.Domain.Posts;
using Backend.Domain.Users;
using Backend.Infrastructure.Hubs;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using Moq;
using MongoDB.Driver;
using Backend.Domain.Notifications;
using Tests.TestFixtures;
using Backend.Infrastructure.Notifications;

namespace Tests.Integration;

[Collection("MongoDB")]
public class NotificationsOrderingTests
{
    private readonly MongoDbFixture _fixture;

    public NotificationsOrderingTests(MongoDbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Notify_Is_Sent_After_Persistence_Completes()
    {
        var owner = MockData.CreateFakeUser();
        var commenter = MockData.CreateFakeUser();

        await _fixture.Context.Users.InsertOneAsync(owner);
        await _fixture.Context.Users.InsertOneAsync(commenter);

        var post = new Post.Builder()
            .WithUserId(owner.Id)
            .WithUsername(owner.Username)
            .WithText("Ordering test post")
            .WithUser(owner)
            .Build();

        var postRepo = new PostRepository(_fixture.Context, Mock.Of<ILogger<PostRepository>>());
        await postRepo.Create(post);

        var comment = post.AddComment(commenter, "Ordering test comment");
        var commentRepo = new CommentRepository(_fixture.Context, Mock.Of<ILogger<CommentRepository>>());
        await commentRepo.Create(post.Id, comment);

        var realNotificationRepo = new NotificationRepository(_fixture.Context, Mock.Of<ILogger<NotificationRepository>>());
        var delayingRepo = new DelayingNotificationRepository(realNotificationRepo, delayMs: 200);

        var mockHubContext = new Mock<IHubContext<NotificationHub>>();
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();

        mockClientProxy
            .Setup(p => p.SendCoreAsync(
                It.Is<string>(s => s == "ReceiveCommentNotification"),
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, object[], CancellationToken>((method, args, ct) =>
            {
                var ownerFromDb = _fixture.Context.Users.Find(u => u.Id == owner.Id).FirstOrDefault();
                ownerFromDb.Should().NotBeNull();
                ownerFromDb!.HasNewNotification.Should().BeTrue();
                ownerFromDb.Notifications.Should().Contain(n => n.Message == comment.Text);
            })
            .Returns(Task.CompletedTask);

        mockClients.Setup(c => c.User(It.IsAny<string>())).Returns(mockClientProxy.Object);
        mockHubContext.SetupGet(h => h.Clients).Returns(mockClients.Object);

        var notifier = new NotificationNotifier(mockHubContext.Object, Mock.Of<ILogger<NotificationNotifier>>());

        var handler = new SendNotificationPostOwnerHandler(
            new UserRepository(_fixture.Context, Mock.Of<ILogger<UserRepository>>()),
            postRepo,
            commentRepo,
            delayingRepo,
            notifier
        );

        var evt = new CommentCreatedEvent(post.Id.Value, comment.Id.Value, commenter.Id.Value);
        await handler.Handle(evt);

        mockClientProxy.Verify(
            p => p.SendCoreAsync(
                It.Is<string>(s => s == "ReceiveCommentNotification"),
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private class DelayingNotificationRepository : INotificationRepository
    {
        private readonly NotificationRepository _inner;
        private readonly int _delayMs;
        public DelayingNotificationRepository(NotificationRepository inner, int delayMs = 100)
        {
            _inner = inner;
            _delayMs = delayMs;
        }
        public async Task<IEnumerable<Notification>> GetNotifications(UserId userId, int pageNumber)
        {
            return await _inner.GetNotifications(userId, pageNumber);
        }

        public async Task<Notification> Create(UserId userId, Notification notification)
        {
            await Task.Delay(_delayMs);
            return await _inner.Create(userId, notification);
        }

        public async Task Delete(UserId userId, Notification notification)
        {
            await _inner.Delete(userId, notification);
        }
    }
}
