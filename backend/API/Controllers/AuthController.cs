using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Backend.Application.Users.CreateByOAuth;
using Backend.Application.Services;
using Backend.Application.Users.Login;
using Backend.Application.Users.ForgotPassword;
using Backend.Application.Users.ResetPassword;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITokenService _tokenService;
    private readonly string _frontend;
    private const string DEFAULT_ROLE = "User";
    public AuthController(
        IMediator mediator,
        ITokenService tokenService,
        IConfiguration configuration)
    {
        _mediator = mediator;
        _tokenService = tokenService;
        _frontend = Environment.GetEnvironmentVariable("FRONTEND")
                    ?? configuration["Frontend"]
                    ?? throw new InvalidOperationException("Frontend is not configured.");
    }


    [HttpGet("signin-oauth/{provider}")]
    public IActionResult SignInWithOAuth(string provider)
    {
        var redirectUrl = Url.Action(nameof(OAuthCallback), new { provider });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

        return Challenge(properties, provider);
    }

    [HttpGet("oauth-callback/{provider}")]
    public async Task<IActionResult> OAuthCallback(string provider)
    {
        var authResult = await HttpContext.AuthenticateAsync(provider);
        if (!authResult.Succeeded)
            return Unauthorized(new { Message = $"{provider} authentication failed." });

        var userId = authResult.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? authResult.Principal.FindFirst("sub")?.Value
                    ?? throw new Exception("Cannot determine provider user ID.")!;
        var email = authResult.Principal.FindFirst(ClaimTypes.Email)?.Value!;
        var userName = authResult.Principal.FindFirst(ClaimTypes.Name)?.Value!;
        var userGuid = ToGuid(userId);
        await _mediator.Send(new CreateUserByOAuthCommand(userGuid, email, userName, provider));

        var token = _tokenService.GenerateToken(userGuid, email, userName, DEFAULT_ROLE);
        var redirectUrl = $"{_frontend}#token={token}";

        return Redirect(redirectUrl);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        if (command == null) return BadRequest("Invalid login data.");
        var user = await _mediator.Send(command)!;
        var token = _tokenService.GenerateToken(user!.Id.Value, user.Email, user.Username, user.Role.ToString());

        return Ok(new
        {
            success = true,
            data = new { token } }
        );
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        if (command == null) return BadRequest("Invalid forgot password data.");
        await _mediator.Send(command);
        return Ok(new
        {
            success = true,
            message = "If an account with that email exists, a password reset OTP has been sent."
        });
    } 

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        if (command == null) return BadRequest("Invalid reset password data.");
        await _mediator.Send(command);
       return NoContent();
    } 


    private Guid ToGuid(string input)
    {
        using var provider = System.Security.Cryptography.MD5.Create();
        var hash = provider.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}