using System.Security.Claims;
using Backend.Application.DTOs.Notifications;
using Backend.Application.Notifications.GetNotifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/users/me/notifications")]
public class NotificationController : ControllerBase
{
    private readonly IMediator _mediator;
    public NotificationController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetNotifications([FromQuery]  int pageNumber)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
        
        var noticaitions = await _mediator.Send(new GetNotificationsQuery(new Guid(ownerId), pageNumber));
        return Ok(new
        {
            success = true,
            data = noticaitions.Select(n => (NotificationDTO) n)
        });
    }

}