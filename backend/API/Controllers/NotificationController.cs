using System.Security.Claims;
using Backend.Application.Features.Notifications.GetNofications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotificationController(IMediator mediator){
            _mediator = mediator;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetNotifications(){
            var authId = GetAuthId();
            var notification = await _mediator.Send(new GetNoficationsCommand(authId));
            return Ok(notification);
        }

        // [Authorize]
        // [HttpGet("{notificationId}")]
        // public async Task<IActionResult> GetNotification(int notificationId){
            
            
        // }

        // [Authorize]
        // [HttpPatch("{notificationId}")]
        // public async Task<IActionResult> MarkNotificationsync(int notificationId){
        //     var notification = await _notificationService.GetNotificationByIdAsync(notificationId);
        //     if (notification == null)
        //         return NotFound(new { message = "Notification not found." });
        //     if (notification.userId != GetAuthId())
        //         return Forbid();
        //     var isUpdated = await _notificationService.MarkNotificationsync(notificationId, !notification.isRead);
        //     if (isUpdated)
        //         return StatusCode(500, new { message = "An error occurred while attempting to update the notification." });
        //     return Ok(new { message = $"Notification with ID {notificationId} has been updated successfully."});
        // }

        // [Authorize]
        // [HttpDelete("{notificationId}")]
        // public async Task<IActionResult> DeleteNotification(int notificationId)
        // {
        //     var notification = await _notificationService.GetNotificationByIdAsync(notificationId);
        //     if (notification == null)
        //         return NotFound(new { message = "Notification not found." });
        //     if (notification.userId != GetAuthId())
        //         return Forbid();

        //     var isDeleted = await _notificationService.SoftDeleteNotificationAsync(notificationId);
        //     if (!isDeleted)
        //         return StatusCode(500, new { message = "An error occurred while attempting to delete the notification." });
        //     return NoContent();
        // }

        private int GetAuthId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userId ?? "0");
        }
    }
}