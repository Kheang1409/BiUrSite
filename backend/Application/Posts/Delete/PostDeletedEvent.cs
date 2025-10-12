
using Backend.Domain.Images;

namespace Backend.Application.Posts.Delete;

public record PostDeletedEvent(
    Guid Id, Image? Image);