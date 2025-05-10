using Backend.Domain.Notifications.Entities;
using Backend.Domain.Notifications.Interfaces;
using MediatR;

namespace Backend.Application.Features.Notifications.GetNofications;
public class GetNoficationsCommandHandler : IRequestHandler<GetNoficationsCommand, List<Notification>>
{
    private readonly INotificationRepository _notificationRepository;

    public GetNoficationsCommandHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<List<Notification>> Handle(GetNoficationsCommand request, CancellationToken cancellationToken)
    {
        var posts = await _notificationRepository.GetNotificationsByUserIdAsync(request.UserId);
        return posts;
    }
}
