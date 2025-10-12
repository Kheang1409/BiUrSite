using MongoDB.Bson.Serialization;
using Backend.Domain.Users;
using Backend.Infrastructure.Serialization;
using Backend.Domain.Posts;
using Backend.Domain.Comments;
using Backend.Domain.Images;

namespace Backend.Infrastructure.Configurations
{
    public static class StronglyTypedIdSerializationRegistry
    {
        private static bool _isRegistered = false;

        public static void Register()
        {
            if (_isRegistered)
                return;

            BsonSerializer.RegisterSerializer(new StronglyTypedIdSerializer<UserId>(
                guid => new UserId(guid),
                id => id.Value
            ));
            BsonSerializer.RegisterSerializer(new StronglyTypedIdSerializer<PostId>(
                guid => new PostId(guid),
                id => id.Value
            ));
            BsonSerializer.RegisterSerializer(new StronglyTypedIdSerializer<CommentId>(
                guid => new CommentId(guid),
                id => id.Value
            ));

            BsonSerializer.RegisterSerializer(new StronglyTypedIdSerializer<ImageId>(
                guid => new ImageId(guid),
                id => id.Value
            ));
            _isRegistered = true;
        }
    }
}