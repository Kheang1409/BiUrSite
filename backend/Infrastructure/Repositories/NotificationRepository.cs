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
            .Where(n => n.Status == Status.Active);

        return notifications;
    }


    public async Task<Notification> Create(UserId userId, Notification notification)
    {
        var update = Builders<User>.Update.PushEach(
        SUB_DOCUMENT_NAME,
        new[] { notification },
            position: 0
        );
        // also mark that the user has new notifications
        var combinedUpdate = Builders<User>.Update.Combine(
            update,
            Builders<User>.Update.Set(u => u.HasNewNotification, true)
        );

        await _users.UpdateOneAsync(
            Builders<User>.Filter.Eq(u => u.Id, userId),
            combinedUpdate
        );

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