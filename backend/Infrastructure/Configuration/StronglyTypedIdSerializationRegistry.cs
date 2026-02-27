using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Backend.Domain.Users;
using Backend.Infrastructure.Serialization;
using Backend.Domain.Posts;
using Backend.Domain.Comments;
using Backend.Domain.Images;
using Backend.Domain.Notifications;
using MongoDB.Bson;

namespace Backend.Infrastructure.Configurations
{
    public static class StronglyTypedIdSerializationRegistry
    {
        private static bool _isRegistered = false;

        public static void Register()
        {
            if (_isRegistered)
                return;

            RegisterGuidSerializer();
            RegisterStronglyTypedIdSerializers();
            RegisterEntityClassMaps();

            _isRegistered = true;
        }

        private static void RegisterGuidSerializer()
        {
            try
            {
                BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            }
            catch (BsonSerializationException)
            {
            }
        }

        private static void RegisterStronglyTypedIdSerializers()
        {
            try { BsonSerializer.RegisterSerializer(new StronglyTypedIdSerializer<UserId>(guid => new UserId(guid), id => id.Value)); } catch (BsonSerializationException) { }
            try { BsonSerializer.RegisterSerializer(new StronglyTypedIdSerializer<NotificationId>(guid => new NotificationId(guid), id => id.Value)); } catch (BsonSerializationException) { }
            try { BsonSerializer.RegisterSerializer(new StronglyTypedIdSerializer<PostId>(guid => new PostId(guid), id => id.Value)); } catch (BsonSerializationException) { }
            try { BsonSerializer.RegisterSerializer(new StronglyTypedIdSerializer<CommentId>(guid => new CommentId(guid), id => id.Value)); } catch (BsonSerializationException) { }
            try { BsonSerializer.RegisterSerializer(new StronglyTypedIdSerializer<ImageId>(guid => new ImageId(guid), id => id.Value)); } catch (BsonSerializationException) { }
        }

        private static void RegisterEntityClassMaps()
        {
            TryRegisterClassMap<User>(cm =>
            {
                cm.AutoMap();
                cm.MapField("_notifications").SetElementName("Notifications");
            });

            TryRegisterClassMap<Post>(cm =>
            {
                cm.AutoMap();
                cm.MapField("_comments").SetElementName("Comments");
                cm.UnmapMember(p => p.User);
            });

            TryRegisterClassMap<Comment>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(c => c.Id);
                cm.MapMember(c => c.UserId);
                cm.UnmapMember(c => c.User);
            });

            TryRegisterClassMap<Notification>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(n => n.User);
            });
        }

        private static void TryRegisterClassMap<T>(Action<BsonClassMap<T>> classMapInitializer)
        {
            try
            {
                if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
                {
                    BsonClassMap.RegisterClassMap(classMapInitializer);
                }
            }
            catch (ArgumentException)
            {
            }
        }
    }
}