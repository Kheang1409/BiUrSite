using MediatR;

namespace Backend.Application.Features.Notifications.CreateNotification;

public record CreateNotificationCommand( string Message, int UserId, int PostId, int CommentId  ) : IRequest<int>;