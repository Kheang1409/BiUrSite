using MediatR;
using Microsoft.AspNetCore.Mvc;
using Backend.Application.Users.CreateUser;
using Backend.Application.Users.GetUser;
using Backend.Application.DTOs;
using Backend.Application.Users.VerifyUser;
using Microsoft.AspNetCore.Authorization;
using Backend.Application.Users.Update;
using System.Security.Claims;

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

    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] UpdateProfileDto dto)
    {
        var emailClaim  = User.FindFirst(ClaimTypes.Email);
        if (emailClaim  is null || string.IsNullOrEmpty(emailClaim .Value))
            return Unauthorized(
                new
                {
                    success = false,
                    message = "User ID claim not found."
                });
        var command = new UpdateProfileCommand(emailClaim.Value, dto.Username, dto.Bio);
        await _mediator.Send(command);
        return NoContent();
    }
    
    // [HttpGet("me-test")]
    // [Authorize]
    // public IActionResult MeTest()
    // {
    //     var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
    //     return Ok(claims);
    // }
}