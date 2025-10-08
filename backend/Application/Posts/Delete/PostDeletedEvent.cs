
using Backend.Domain.Posts;

namespace Backend.Application.Posts.Delete;

public record PostDeletedEvent(
    Guid Id, Image Image);