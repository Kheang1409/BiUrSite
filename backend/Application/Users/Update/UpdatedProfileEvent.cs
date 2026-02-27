
namespace Backend.Application.Users.Update;

public record UpdatedProfileEvent(
    Guid Id, byte[]? Data);