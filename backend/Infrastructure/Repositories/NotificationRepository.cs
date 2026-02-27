using Backend.Domain.Enums;
using Backend.Domain.Notifications;
using Backend.Domain.Users;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Resilience;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Backend.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly IMongoCollection<User> _users;
    private readonly ILogger<NotificationRepository> _logger;
    private const int LIMIT = 10;
    private const string SUB_DOCUMENT_NAME = "Notifications";

    public NotificationRepository(MongoDbContext context, ILogger<NotificationRepository> logger)
    {
        _users = context.Users;
        _logger = logger;
    }

    public async Task<IEnumerable<Notification>> GetNotifications(UserId userId, int pageNumber)
    {
        var skip = (pageNumber - 1) * LIMIT;

        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.Id, userId),
            Builders<User>.Filter.Eq(u => u.Status, Status.Active));

        // Get all notifications and handle pagination in memory with proper sorting
        var user = await RetryPolicy.ExecuteWithRetryAsync(
            () => _users.Find(filter)
                .Project<User>(Builders<User>.Projection.Include(u => u.Id).Include(SUB_DOCUMENT_NAME))
                .FirstOrDefaultAsync(),
            logger: _logger,
            operationName: "GetNotifications");

        if (user?.Notifications == null)
            return Enumerable.Empty<Notification>();

        var notifications = user.Notifications
            .Where(n => n.Status == Status.Active)
            .OrderByDescending(n => n.CreatedDate)
            .Skip(skip)
            .Take(LIMIT)
            .ToList();

        var userIds = notifications.Select(n => n.UserId).Distinct().ToList();
        if (userIds.Count > 0)
        {
            var users = await RetryPolicy.ExecuteWithRetryAsync(
                () => _users.Find(Builders<User>.Filter.In(u => u.Id, userIds)).ToListAsync(),
                logger: _logger,
                operationName: "GetNotifications_FetchUsers");
            
            var userById = users.ToDictionary(u => u.Id, u => u);
            foreach (var notification in notifications)
            {
                if (userById.TryGetValue(notification.UserId, out var notifUser))
                {
                    notification.SetUser(notifUser);
                }
            }
        }

        return notifications;
    }

    public async Task<Notification> Create(UserId userId, Notification notification)
    {
        var notiMatchFilter = Builders<Notification>.Filter.And(
            Builders<Notification>.Filter.Eq(n => n.PostId, notification.PostId),
            Builders<Notification>.Filter.Eq(n => n.Message, notification.Message),
            Builders<Notification>.Filter.Eq(n => n.Status, Status.Active));

        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.Id, userId),
            Builders<User>.Filter.Not(
                Builders<User>.Filter.ElemMatch(SUB_DOCUMENT_NAME, notiMatchFilter)));

        var update = Builders<User>.Update
            .PushEach(SUB_DOCUMENT_NAME, new[] { notification }, position: 0)
            .Set(u => u.HasNewNotification, true);

        var result = await RetryPolicy.ExecuteWithRetryAsync(
            () => _users.UpdateOneAsync(filter, update),
            logger: _logger,
            operationName: "CreateNotification");

        if (result.ModifiedCount > 0)
        {
            return notification;
        }

        var existingFilter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.Id, userId),
            Builders<User>.Filter.ElemMatch(SUB_DOCUMENT_NAME, notiMatchFilter));

        var projection = Builders<User>.Projection
            .ElemMatch(SUB_DOCUMENT_NAME, notiMatchFilter);

        var bsonResult = await RetryPolicy.ExecuteWithRetryAsync(
            () => _users.Find(existingFilter).Project<BsonDocument>(projection).FirstOrDefaultAsync(),
            logger: _logger,
            operationName: "CreateNotification_FindExisting");

        if (bsonResult != null && bsonResult.Contains(SUB_DOCUMENT_NAME) && bsonResult[SUB_DOCUMENT_NAME].AsBsonArray.Count > 0)
        {
            var doc = bsonResult[SUB_DOCUMENT_NAME].AsBsonArray[0].AsBsonDocument;
            return BsonSerializer.Deserialize<Notification>(doc);
        }
        return notification;
    }

    public Task Delete(UserId userId, Notification notification)
    {
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.Id, userId),
            Builders<User>.Filter.ElemMatch(SUB_DOCUMENT_NAME, Builders<Notification>.Filter.Eq("Id", notification.Id)));

        var update = Builders<User>.Update
            .Set($"{SUB_DOCUMENT_NAME}.$.Status", notification.Status)
            .Set($"{SUB_DOCUMENT_NAME}.$.DeletedDate", notification.DeletedDate);

        return RetryPolicy.ExecuteWithRetryAsync(
            () => _users.UpdateOneAsync(filter, update),
            logger: _logger,
            operationName: "DeleteNotification");
    }
}