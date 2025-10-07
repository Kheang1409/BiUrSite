
using Backend.Domain.Posts;

namespace Backend.Application.Posts.Delete;

public record PostDeletedEvent(
    string Id, Image Image);