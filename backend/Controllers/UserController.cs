using Backend.DTOs;
using Backend.Models;
using Backend.Repositories;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        private readonly INotificationService _notificationService;
        public UserController(IUserService userService, IJwtService jwtService, INotificationService notificationService)
        {
            _userService = userService;
            _jwtService = jwtService;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Users([FromQuery] int ? pageNumber=1, [FromQuery] string? username = null){
            IActionResult response = Ok(new {message = "really?"});
            if(pageNumber < 1)
                response = BadRequest(new { message = "Page number cannot negative!"});
            if(response is OkObjectResult){
                int _pageNumber = (pageNumber ?? 1) - 1;
                var users = await _userService.GetUsersAsync(_pageNumber, username);
                response = Ok(users);
            }
            
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            IActionResult response = Ok(new { message = "Your account has been created! Please check your email to verify your account." });
            
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                       .Select(e => e.ErrorMessage).ToList();
                response = BadRequest(new { message = "Invalid input", errors});
            }
            if (await _userService.GetUserByEmailAsync(dto.email) != null)
            {
                response = BadRequest(new { message = "Email is already registered!", statusCode = 400 });
            }
            if(response is OkObjectResult)
            {
                var user = new User
                {
                    username = dto.username,
                    email = dto.email,
                    password = HashPassword(dto.password),
                    verificationToken = GenerateVerfiedToken(),
                    verificationTokenExpiry = DateTime.UtcNow.AddHours(24)
                };
                var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
                var verificationLink = $"{baseUrl}/api/users/verify-user?verificationToken={user.verificationToken}";
                Console.WriteLine($"Verification Link: {verificationLink}");
                await _userService.AddUserAsync(user);
                await _notificationService.SendConfirmationEmail(user.email, verificationLink);
            }
            return response;
        }

        [HttpGet("verify-user")]
        public async Task<IActionResult> VerifyUser([FromQuery] string verificationToken)
        {
            IActionResult response = Ok(new { message = "Verified successfully." });
            if (!ModelState.IsValid)
            {
                response = BadRequest(new { message = "Invalid input", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
            }

            var user = await _userService.GetUserByVerificationTokenAsync(verificationToken);
            if (user == null)
                response = BadRequest(new { message = "Invalid or expired verification Token.", statusCode = 400 });

            if(response is OkObjectResult){
                user.verificationToken = string.Empty;
                user.verificationTokenExpiry = null;
                user.status = "Verified";
                await _userService.UpdateUserAsync(user);
            }
            return response;
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgetPasswordDto forgetPasswordDto)
        {
            IActionResult response = Ok(new { message = "OTP has been sent to your email." });
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                       .Select(e => e.ErrorMessage).ToList();
                response = BadRequest(new { message = "Invalid input", errors});
            }

            var email = forgetPasswordDto.Email;
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
                response = BadRequest("Email not registered.");
            if(response is OkObjectResult){
                user.opt = GenerateOtp();
                user.optExpiry = DateTime.UtcNow.AddMinutes(3);
                await _userService.UpdateUserAsync(user);
                await _notificationService.SendOtpEmail(user.email, user.opt);
            }
            return response;
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            IActionResult response = Ok(new { message = "Password reset successfully." });
            if (!ModelState.IsValid)
            {
                response = BadRequest(new { message = "Invalid input", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
            }

            var user = await _userService.GetUserByOPTAsync(dto.Otp);
            if (user == null)
                response = BadRequest(new { message = "Invalid or expired OTP.", statusCode = 400 });

            if(response is OkObjectResult){
                user.password = HashPassword(dto.NewPassword);
                user.opt = string.Empty;
                user.optExpiry = null;
                await _userService.UpdateUserAsync(user);
            }
            

            return response;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            IActionResult response = Ok(new {});
            if (!ModelState.IsValid)
            {
                response = BadRequest(new { message = "Invalid input", });
            }

            var user = await _userService.GetUserByEmailAsync(dto.Email);
            if (user == null || !VerifyPassword(dto.Password, user.password))
            {
                response = Unauthorized(new { message = "Invalid email or password." });
            }
            if(response is OkObjectResult){
                var token = _jwtService.GenerateToken(user.userId, user.email, user.username, user.role);
                response = Ok(new { Token = token });
            }
            return response;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{userId}/ban")]
        public async Task<IActionResult> Ban(int userId){
            IActionResult response = Ok(new { message = $"user with an Id: {userId} is banned!" });
            var user = await _userService.GetUserByIdAsync(userId);
            if(user == null){
                response = BadRequest(new { message = "ban user unsuccessfully!"});
            }
            if(response is OkObjectResult){
                user.status = "Banned";
                await _userService.UpdateUserAsync(user);
            }
            return response;
        }

        private static string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private static string GenerateVerfiedToken()
        {
            var verificationToken = Guid.NewGuid().ToString(); 
            return verificationToken;
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        private static bool VerifyPassword(string password, string passwordHash)
        {
            using var sha256 = SHA256.Create();
            var hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
            return hash == passwordHash;
        }
    }
}
