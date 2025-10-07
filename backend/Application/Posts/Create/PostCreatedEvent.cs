
namespace Backend.Application.Posts.Create;

public record PostCreatedEvent(
    string Id, byte[]? Data);