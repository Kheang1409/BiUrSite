using Backend.Application.Features.Users.RegisterUser;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Backend.Application.Features.Users.RegisterOAuthUser;
using Microsoft.AspNetCore.Authentication.Cookies;
using Backend.Application.Features.Auth.Login;
using Backend.Application.Features.Auth.Verify;
using Backend.Application.Features.Auth.ForgotPassword;
using Backend.Application.Features.Users.GetUserProfile;
using Backend.Application.Features.Auth.ResetPassword;
using Backend.Application.Features.Users.BanUser;
using Backend.Application.Features.Users.DeleteUser;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly string _frontend;

        public UserController(IMediator mediator, IConfiguration configuration)
        {
            _mediator = mediator;
            _frontend = Environment.GetEnvironmentVariable("FRONTEND") 
                        ?? configuration["Frontend"]
                        ?? throw new InvalidOperationException("Frontend is not configured.");
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
        {
            if (command == null) return BadRequest("Invalid registration data.");
            try
            {
                var result = await _mediator.Send(command);
                return Ok(new { message = "Registration successful. Please check your email to verify your account." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred during registration.", error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            if (command == null) return BadRequest("Invalid login data.");
            try
            {
                var token = await _mediator.Send(command);
                return Ok(new { Token = token });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { Message = "Invalid credentials." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while logging in.", Error = ex.Message });
            }
        }

        [HttpGet("verify")]
        public async Task<IActionResult> VerifyUser([FromQuery] VerifyCommand command)
        {
            try{
                var isVerified = await _mediator.Send(command);
                if (!isVerified)
                    return BadRequest(new { message = "Invalid or expired token. Please request a new one." });
                return Redirect($"{_frontend}/login");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while verifying the account.", error = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
        {
            if (command == null) return BadRequest("Invalid login data.");
            try
            {
                var result = await _mediator.Send(command);
                return Ok(new { message = "Reset password email sent successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while sending the reset password email.", error = ex.Message });
            }
            
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
        {
            if (command == null) return BadRequest("Invalid login data.");
            try
            {
                var result = await _mediator.Send(command);
                if(!result)
                    return BadRequest(new { message = "Invalid or expired token. Please request a new one." });
                return Ok(new { message = "Password reset successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while sending the reset password email.", error = ex.Message });
            }
        }

        

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/ban")]
        public async Task<IActionResult> BanUser(int id)
        { 
            try
            {
                var result = await _mediator.Send(new BanUserCommand(id));
                if (!result)
                    return NotFound(new { message = "User not found." });
                return Ok(new { message = "User banned successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while banning the user.", error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try{
                var result = await _mediator.Send(new DeleteUserCommand(id));
                if (!result)
                    return NotFound(new { message = "User not found." });
                return Ok(new { message = "User deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the user.", error = ex.Message });
            }
        }

        [HttpGet("signin-google")]
        public IActionResult SignInWithGoogle()
        {
            var properties = new AuthenticationProperties { RedirectUri = "/api/users/google-callback" };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            var authResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!authResult.Succeeded)
                return Unauthorized(new { Message = "Google authentication failed." });

            var email = authResult.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = authResult.Principal.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { Message = "Email not found in Google response." });
            try
            {
                var existingUser = await _mediator.Send(new GetUserProfileCommand(email));
                if(existingUser == null){
                    await _mediator.Send(new RegisterOAuthUserCommand(email, name ?? "Unknown", "X7p$k9Tr@1qL"));
                }
                var token  = await _mediator.Send(new LoginCommand(email, "X7p$k9Tr@1qL"));
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    authResult.Principal, new AuthenticationProperties { IsPersistent = true });

                return Redirect($"{_frontend}/oauth-callback?token={token}&provider=google");
            }
            catch (Exception)
            {
                return Redirect($"{_frontend}/login?error=An error occurred during Google login");
            }
        }

        [HttpGet("signin-facebook")]
        public IActionResult SignInWithFacebook()
        {
            var properties = new AuthenticationProperties { RedirectUri = "/api/users/facebook-callback" };
            return Challenge(properties, FacebookDefaults.AuthenticationScheme);
        }

        [HttpGet("facebook-callback")]
        public async Task<IActionResult> FacebookCallback()
        {
            var authResult = await HttpContext.AuthenticateAsync(FacebookDefaults.AuthenticationScheme);
            if (!authResult.Succeeded)
                return Unauthorized(new { Message = "Facebook authentication failed." });

            var email = authResult.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = authResult.Principal.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { Message = "Email not found in Facebook response." });
            try
            {
                var existingUser = await _mediator.Send(new GetUserProfileCommand(email));
                if(existingUser == null){
                    await _mediator.Send(new RegisterOAuthUserCommand(email, name ?? "Unknown", $"X7p$k9Tr@1qL"));
                }
                var token = await _mediator.Send(new LoginCommand(email, "X7p$k9Tr@1qL"));
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    authResult.Principal, new AuthenticationProperties { IsPersistent = true });

                return Redirect($"{_frontend}/oauth-callback?token={token}&provider=google");
            }
            catch (Exception)
            {
                return Redirect($"{_frontend}/login?error=An error occurred during Google login");
            }
        }
    }
}