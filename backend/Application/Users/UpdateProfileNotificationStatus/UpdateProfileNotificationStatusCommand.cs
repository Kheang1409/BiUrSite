using MediatR;

namespace Backend.Application.Users.UpdateProfileNotificationStatus;

public record UpdateProfileNotificationStatusCommand(
    string Email) : IRequest;
