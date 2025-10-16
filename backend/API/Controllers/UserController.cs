using MediatR;
using Microsoft.AspNetCore.Mvc;
using Backend.Application.Users.Create;
using Backend.Application.Users.GetUser;
using Backend.Application.Users.VerifyUser;
using Microsoft.AspNetCore.Authorization;
using Backend.Application.Users.Update;
using System.Security.Claims;
using Backend.Application.DTOs.Users;
using Backend.Application.Users.Delete;
using Backend.Application.Users.UpdateProfileNotificationStatus;
using Backend.Application.Users.GetUsers;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    public UserController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] CreateUserCommand command)
    {
        if (command == null) return BadRequest("Invalid registration data.");
        var user = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id.Value }, new
        {
            success = true,
            message = "User registered successfully. Please check your email to verify your account."
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query)
    {
        var users = await _mediator.Send(query);
        return Ok(new
        {
            success = true,
            data = users.Select(user => (UserDto)user)
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var query = new GetUserByIdQuery(id);
        var user = await _mediator.Send(query);
        return Ok(new
        {
            success = true,
            data = (UserDto)user!
        });
    }

    [HttpGet("verify")]
    public async Task<IActionResult> Verify([FromQuery] string token)
    {
        var command = new VerifyUserCommand(token);
        await _mediator.Send(command);
        return Ok(new
        {
            success = true,
            message = "Your account has been verified successfully."
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
        
        var user = await _mediator.Send(new GetUserByIdQuery(new Guid(userId)));
        return Ok(new
        {
            success = true,
            data = (UserDto)user!
        });
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] UpdateProfileDto dto)
    {
        var emailClaim = User.FindFirst(ClaimTypes.Email);
        if (emailClaim is null || string.IsNullOrEmpty(emailClaim.Value))
            return Unauthorized(
                new
                {
                    success = false,
                    message = "User ID claim not found."
                });
        var command = new UpdateProfileCommand(emailClaim.Value, dto.Username, dto.Bio, dto.Data);
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPatch("me")]
    [Authorize]
    public async Task<IActionResult> MarkNotificationAsRead()
    {
        var emailClaim = User.FindFirst(ClaimTypes.Email);
        if (emailClaim is null || string.IsNullOrEmpty(emailClaim.Value))
            return Unauthorized(
                new
                {
                    success = false,
                    message = "User ID claim not found."
                });
        var command = new UpdateProfileNotificationStatusCommand(emailClaim.Value);
        await _mediator.Send(command);
        return NoContent();
    }
    
    [HttpDelete("me")]
    [Authorize]
    public async Task<IActionResult> Delete()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
        await _mediator.Send(new DeleteUserCommand(new Guid(userId)));
        return Ok(new
        {
            success = true,
            message = "Your account has been deleted successfully."
        });
    }
    
    [HttpGet("me-test")]
    [Authorize]
    public IActionResult MeTest()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        return Ok(claims);
    }
}