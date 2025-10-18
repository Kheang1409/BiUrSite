using Backend.Domain.Enums;
using Backend.Domain.Notifications;
using Backend.Domain.Posts;
using Backend.Domain.Users;
using Backend.Infrastructure.Persistence;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Backend.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly IMongoCollection<User> _users;
    private const int LIMIT = 10;
    private const string SUB_DOCUMENT_NAME = "Notifications";

    public NotificationRepository(
        MongoDbContext context
        )
    {
        _users = context.Users;
    }

    public async Task<IEnumerable<Notification>> GetNotifications(UserId postId, int pageNumber)
    {
        var skip = (pageNumber - 1) * LIMIT;

        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.Id, postId),
            Builders<User>.Filter.Eq(u => u.Status, Status.Active)
        );

        var projection = Builders<User>.Projection
            .Slice(SUB_DOCUMENT_NAME, skip, LIMIT)
            .Include(p => p.Id);

        var bsonResult = await _users
            .Find(filter)
            .Project<BsonDocument>(projection)
            .FirstOrDefaultAsync();

        if (bsonResult == null)
            return Enumerable.Empty<Notification>();

        var notifications = bsonResult[SUB_DOCUMENT_NAME]
            .AsBsonArray
            .Select(n => BsonSerializer.Deserialize<Notification>(n.AsBsonDocument))
            .Where(n => n.Status == Status.Active)
            .ToList();

        foreach (var notification in notifications)
        {
            var user = await _users.Find(Builders<User>.Filter.Eq(u => u.Id, notification.UserId)).FirstOrDefaultAsync();
            notification.SetUser(user);
        }

        return notifications;
    }


    public async Task<Notification> Create(UserId userId, Notification notification)
    {
        var notiMatchFilter = Builders<Notification>.Filter.And(
            Builders<Notification>.Filter.Eq(n => n.PostId, notification.PostId),
            Builders<Notification>.Filter.Eq(n => n.Message, notification.Message),
            Builders<Notification>.Filter.Eq(n => n.Status, Status.Active)
        );

        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.Id, userId),
            Builders<User>.Filter.Not(
                Builders<User>.Filter.ElemMatch(SUB_DOCUMENT_NAME, notiMatchFilter)
            )
        );

        var update = Builders<User>.Update
            .PushEach(SUB_DOCUMENT_NAME, new[] { notification }, position: 0)
            .Set(u => u.HasNewNotification, true);

        var result = await _users.UpdateOneAsync(filter, update);

        if (result.ModifiedCount > 0)
        {
            return notification;
        }

        var existingFilter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.Id, userId),
            Builders<User>.Filter.ElemMatch(SUB_DOCUMENT_NAME, notiMatchFilter)
        );

        var projection = Builders<User>.Projection
            .ElemMatch(SUB_DOCUMENT_NAME, notiMatchFilter);

        var bsonResult = await _users
            .Find(existingFilter)
            .Project<BsonDocument>(projection)
            .FirstOrDefaultAsync();

        if (bsonResult != null && bsonResult.Contains(SUB_DOCUMENT_NAME) && bsonResult[SUB_DOCUMENT_NAME].AsBsonArray.Count > 0)
        {
            var doc = bsonResult[SUB_DOCUMENT_NAME].AsBsonArray[0].AsBsonDocument;
            return BsonSerializer.Deserialize<Notification>(doc);
        }
        return notification;
    }

    public async Task Delete(UserId userId, Notification notification)
    {
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.Id, userId),
            Builders<User>.Filter.ElemMatch(SUB_DOCUMENT_NAME, Builders<Notification>.Filter.Eq("Id", notification.Id))
        );

        var update = Builders<User>.Update
            .Set($"{SUB_DOCUMENT_NAME}.$.Status", notification.Status)
            .Set($"{SUB_DOCUMENT_NAME}.$.DeletedDate", notification.DeletedDate);

        await _users.UpdateOneAsync(filter, update);
    }
}