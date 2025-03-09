using AutoMapper;
using Backend.DTOs;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly ICacheService _cache;

        public UserController(IUserService userService, IJwtService jwtService, INotificationService notificationService, IMapper mapper, ICacheService cache, IConnectionMultiplexer redis)
        {
            _userService = userService;
            _jwtService = jwtService;
            _notificationService = notificationService;
            _mapper = mapper;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] int? pageNumber = 1, [FromQuery] string? username = null)
        {
            if (pageNumber < 1)
                return BadRequest(new { message = "Page number must start from 1." });

            var cacheKey = $"users_page_{pageNumber}_{username ?? "all"}";
            int _pageNumber = (pageNumber ?? 1) - 1;
            var users = await _cache.GetDataAsync<List<User>>(cacheKey) ?? await _userService.GetUsersAsync(_pageNumber, username);
            await _cache.SetDataAsync(cacheKey, users, TimeSpan.FromMinutes(5));

            return Ok(new { message = "User data retrieved.", data = _mapper.Map<List<UserDto>>(users) });
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser(int userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(new { message = "User data retrieved.", data = _mapper.Map<UserDto>(user) });
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { message = "Invalid input. Please check the provided data and try again.", errors });
            }

            var existUser = await _userService.GetUserByEmailAsync(registerDto.email);
            if (existUser != null)
                return BadRequest(new { message = "This email is already registered. Please use a different email or log in." });

            var user = _mapper.Map<User>(registerDto).GenerateVerfiedToken();
            user.password = Models.User.HashPassword(user.password);

            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            var verificationLink = $"{baseUrl}/api/users/verify-user?verificationToken={user.verificationToken}";

            await Task.WhenAll(
                _userService.AddUserAsync(user),
                _notificationService.SendConfirmationEmail(user.email, verificationLink)
            );

            return StatusCode(201, new { message = "Account created successfully. Please check your email to verify your account." });
        }

        [HttpGet("verify-user")]
        public async Task<IActionResult> VerifyUser([FromQuery] string verificationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid input. Please check the provided data and try again.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });

            var isVerified = await _userService.UserVerifiedAsync(verificationToken);
            if (!isVerified)
                return BadRequest(new { message = "The verification token is either invalid or has expired. Please request a new verification email." });

            return Ok(new { message = "Account verified successfully." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgetPasswordDto forgetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { message = "Invalid input. Please check the provided data and try again.", errors });
            }

            var email = forgetPasswordDto.Email;
            var existUser = await _userService.GetUserByEmailAsync(email);
            if (existUser == null)
                return NotFound(new { message = "No account found with this email address. Please check the email or register a new account." });

            existUser.GenerateOtp();
            var isValid = await _userService.UserForgetPasswordAsync(email, existUser.opt);
            if (!isValid)
                return StatusCode(500, new { message = "An error occurred while processing your request. Please try again later." });

            await _notificationService.SendOtpEmail(existUser.email, existUser.opt);
            return Ok(new { message = "A One-Time Password (OTP) has been sent to your email." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid input. Please check the provided data and try again.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });

            var hashPassword = Models.User.HashPassword(resetPasswordDto.newPassword);
            var isReset = await _userService.UserResetPasswordAsync(resetPasswordDto.otp, hashPassword);
            if (!isReset)
                return BadRequest(new { message = "The OTP is either invalid or has expired. Please request a new OTP." });

            return Ok(new { message = "Password has been reset successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid input. Please check the provided details and try again." });

            var user = await _userService.GetUserByEmailAsync(loginDto.email);
            if (user == null || !user.VerifyPassword(loginDto.password))
                return Unauthorized(new { message = "Invalid email or password." });

            var token = _jwtService.GenerateToken(user);
            return Ok(new { Token = token });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{userId}/ban")]
        public async Task<IActionResult> Ban(int userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var isBanned = await _userService.BanUserAsync(userId);
            if (!isBanned)
                return StatusCode(500, new { message = "An error occurred while attempting to ban the user." });

            return Ok(new { message = $"User with ID {userId} has been banned successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{userId}")]
        public async Task<IActionResult> Delete(int userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var isDeleted = await _userService.SoftDeleteUserAsync(userId);
            if (!isDeleted)
                return StatusCode(500, new { message = "An error occurred while attempting to delete the user." });

            return NoContent();
        }
    }
}