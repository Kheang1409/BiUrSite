using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Domain.Shared;

public record Image
{
    [BsonId]
    public string Id { get; private set; }
    public string Url { get; private set; }

    public Image(string id, string url)
    {
        Id = id;
        Url = url;
    }
}