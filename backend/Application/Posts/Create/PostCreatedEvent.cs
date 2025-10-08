
namespace Backend.Application.Posts.Create;

public record PostCreatedEvent(
    Guid Id, byte[]? Data);