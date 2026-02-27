using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Infrastructure.Outbox;

public sealed class OutboxMessage
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; init; }
    
    public string EventType { get; init; } = string.Empty;
    
    public string Payload { get; init; } = string.Empty;
    
    public DateTime OccurredOnUtc { get; init; }
    
    public DateTime? ProcessedOnUtc { get; set; }
    
    public string? Error { get; set; }
    
    public int RetryCount { get; set; }
    
    public bool IsProcessed => ProcessedOnUtc.HasValue;
}
