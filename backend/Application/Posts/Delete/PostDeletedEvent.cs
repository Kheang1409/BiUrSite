
using Backend.Domain.Shared;

namespace Backend.Application.Posts.Delete;

public record PostDeletedEvent(
    Guid Id, Image Image);