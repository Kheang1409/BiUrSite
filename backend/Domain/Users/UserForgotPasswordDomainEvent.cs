using Backend.Domain.Primitive;

namespace Backend.Domain.Users;

public record UserForgotPasswordDomainEvent(
    Guid Id,
    UserId UserId) : DomainEvent(Id, DateTime.UtcNow);