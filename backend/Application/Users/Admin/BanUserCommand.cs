using MediatR;

namespace Backend.Application.Users.Admin;

public record BanUserCommand(
    Guid UserId,
    string? Reason,
    int? DurationMinutes) : IRequest;
