using Google.Apis.Auth;
using AutoMapper;
using Backend.DTOs;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ICacheService _cache;

        public UserController(IUserService userService, IJwtService jwtService, IEmailService emailService, IMapper mapper, ICacheService cache)
        {
            _userService = userService;
            _jwtService = jwtService;
            _emailService = emailService;
            _mapper = mapper;
            _cache = cache;
        }

        // Get list of users with pagination and optional filter by username
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] int? page = 1, [FromQuery] string? username = null)
        {
            if (page < 1)
                return BadRequest(new { message = "Page number must start from 1." });

            var cacheKey = $"users_page_{page}_{username ?? "all"}";
            var users = await _cache.GetDataAsync<List<User>>(cacheKey) ?? await _userService.GetUsersAsync(page.Value - 1, username);
            await _cache.SetDataAsync(cacheKey, users, TimeSpan.FromMinutes(5));

            return Ok(new { message = "Users retrieved successfully.", data = _mapper.Map<List<UserDto>>(users) });
        }

        // Get a single user by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(new { message = "User data retrieved successfully.", data = _mapper.Map<UserDto>(user) });
        }

        // Register a new user
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { message = "Invalid input. Please check the provided data.", errors });
            }

            var existingUser = await _userService.GetUserByEmailAsync(registerDto.email);
            if (existingUser != null)
                return BadRequest(new { message = "Email is already registered." });

            var user = _mapper.Map<User>(registerDto).GenerateVerificationToken();
            user.password = Models.User.HashPassword(user.password);
            var verificationLink = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/v1/users/verify?token={user.verificationToken}";

            await Task.WhenAll(
                _userService.AddUserAsync(user),
                _emailService.SendConfirmationEmail(user.email, verificationLink)
            );

            return CreatedAtAction(nameof(GetUserById), new { id = user.userId }, new { message = "Account created. Please verify your email." });
        }

        // Verify user account with token
        [HttpGet("verify")]
        public async Task<IActionResult> VerifyUser([FromQuery] string token)
        {
            var isVerified = await _userService.UserVerifiedAsync(token);
            if (!isVerified)
                return BadRequest(new { message = "Invalid or expired token. Please request a new one." });

            return Ok(new { message = "Account successfully verified." });
        }

        // Request password reset (OTP sent via email)
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgetPasswordDto forgetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { message = "Invalid input. Please check the provided data.", errors });
            }

            var user = await _userService.GetUserByEmailAsync(forgetPasswordDto.email);
            if (user == null)
                return NotFound(new { message = "No account found with this email." });

            user.GenerateOtp();
            await _userService.UserForgetPasswordAsync(user.email, user.otp);
            await _emailService.SendOtpEmail(user.email, user.otp);

            return Ok(new { message = "OTP has been sent to your email for password reset." });
        }

        // Reset password using OTP
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid input. Please check the provided data.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });

            var isReset = await _userService.UserResetPasswordAsync(resetPasswordDto.otp, Models.User.HashPassword(resetPasswordDto.newPassword));
            if (!isReset)
                return BadRequest(new { message = "Invalid or expired OTP. Please request a new OTP." });

            return Ok(new { message = "Password reset successfully." });
        }

        // User login (returns JWT token)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid input. Please check the provided data." });

            var user = await _userService.GetUserByEmailAsync(loginDto.email);
            if (user == null || !user.VerifyPassword(loginDto.password))
                return Unauthorized(new { message = "Invalid email or password." });

            var token = _jwtService.GenerateToken(user);
            return Ok(new { message = "Login successful.", token });
        }

        // Admin: Ban user
        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/ban")]
        public async Task<IActionResult> BanUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var isBanned = await _userService.BanUserAsync(id);
            if (!isBanned)
                return StatusCode(500, new { message = "An error occurred while banning the user." });

            return Ok(new { message = $"User with ID {id} has been banned." });
        }

        // Admin: Soft delete user
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var isDeleted = await _userService.SoftDeleteUserAsync(id);
            if (!isDeleted)
                return StatusCode(500, new { message = "An error occurred while deleting the user." });

            return NoContent();
        }

        [HttpPost("auth/external-login")]
        public async Task<IActionResult> ExternalLogin([FromBody] SocialLoginRequest request)
        {
            if (string.IsNullOrEmpty(request.token))
            {
                return BadRequest(new { message = "Token is required." });
            }

            try
            {
                User existingUser = null;
                string userEmail = null;
                string userName = null;
                string profilePicture = null;

                if (request.provider == "google")
                {
                    // Handle Google OAuth login
                    var googlePayload = await GoogleJsonWebSignature.ValidateAsync(request.token);
                    userEmail = googlePayload.Email;
                    userName = googlePayload.Name;
                    profilePicture = googlePayload.Picture;
                }
                else if (request.provider == "facebook")
                {
                    // Handle Facebook OAuth login
                    var fbClient = new HttpClient();
                    var fbResponse = await fbClient.GetStringAsync($"https://graph.facebook.com/v10.0/me?access_token={request.token}&fields=id,name,email,picture");

                    var fbUser = JsonSerializer.Deserialize<FacebookUserDto>(fbResponse);
                    userEmail = fbUser?.email;
                    userName = fbUser?.name;
                    profilePicture = fbUser?.picture?.data?.url;
                }
                else
                {
                    return BadRequest(new { message = "Unsupported provider" });
                }

                if (string.IsNullOrEmpty(userEmail))
                {
                    return BadRequest(new { message = $"{request.provider} login error: email not found." });
                }

                existingUser = await _userService.GetUserByEmailAsync(userEmail);
                if (existingUser == null)
                {
                    return await CreateUserAndGenerateToken(userEmail, userName, profilePicture, request.provider);
                }

                var jwtToken = _jwtService.GenerateToken(existingUser);
                return Ok(new { message = "Login successful.", token = jwtToken });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Login error", error = ex.Message });
            }
        }

        // Helper method to create user and generate token
        private async Task<IActionResult> CreateUserAndGenerateToken(string email, string name, string profilePicture, string provider)
        {
            var user = new User
            {
                email = email,
                username = name,
                password = Models.User.HashPassword(Guid.NewGuid().ToString()), // Use GUID or other logic for default password
                profile = profilePicture ?? "assets/img/profile-default.svg",
                isActive = true,
                status = Enums.Status.Verified,
                role = Enums.Role.User,
                userSource = provider
            };

            await _userService.AddUserAsync(user);
            var token = _jwtService.GenerateToken(user);
            return Ok(new { message = $"{provider} login successful.", token });
        }
    }
}