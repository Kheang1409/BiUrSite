using Backend.Domain.Notifications.Entities;
using MediatR;

namespace Backend.Application.Features.Notifications.GetNofications;

public record GetNoficationsCommand(int UserId) : IRequest<List<Notification>>;
