using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Domain.Posts;

public record Image
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; private set; }
    public string Url { get; private set; }

    public Image(string id, string url)
    {
        Id = id;
        Url = url;
    }
}