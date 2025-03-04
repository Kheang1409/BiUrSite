using AutoMapper;
using Backend.DTOs;
using Backend.Models;
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
        private readonly IMapper _mapper;
        public UserController(IUserService userService, IJwtService jwtService, INotificationService notificationService, IMapper mapper)
        {
            _userService = userService;
            _jwtService = jwtService;
            _notificationService = notificationService;
            _mapper= mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Users([FromQuery] int ? pageNumber=1, [FromQuery] string? username = null){
            IActionResult response = Ok(new {message = "User data retrieved."});
            if(pageNumber < 1)
                response = BadRequest(new { message = "Page number must be start from 1!"});
            if(response is OkObjectResult){
                int _pageNumber = (pageNumber ?? 1) - 1;
                var users = await _userService.GetUsersAsync(_pageNumber, username);
                response = Ok(users);
            }
            return response;
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            IActionResult response = Ok(new { message = "Account created successfully. Please check your email to verify your account."});
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                       .Select(e => e.ErrorMessage).ToList();
                response = BadRequest(new { message = "Invalid input. Please check the provided data and try again.", errors});
            }
            var existUser = await _userService.GetUserByEmailAsync(registerDto.email);
            if (existUser != null)
            {
                response = BadRequest(new { message = "This email is already registered. Please use a different email or log in.", statusCode = 400 });
            }
            if(response is OkObjectResult)
            {
                var user = createUserObject(_mapper.Map<User>(registerDto));
                var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
                var verificationLink = $"{baseUrl}/api/users/verify-user?verificationToken={user.verificationToken}";
                await _userService.AddUserAsync(user);
                await _notificationService.SendConfirmationEmail(user.email, verificationLink);
            }
            return response;
        }

        [HttpGet("verify-user")]
        public async Task<IActionResult> VerifyUser([FromQuery] string verificationToken)
        {
            IActionResult response = Ok(new { message = "Account verified successfully." });
            if (!ModelState.IsValid)
            {
                response = BadRequest(new { message = "Invalid input. Please check the provided data and try again.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
            }
            var isVerified = await _userService.UserVerified(verificationToken);
            if (!isVerified)
            {
                response = BadRequest(new { message = "The verification token is either invalid or has expired. Please request a new verification email.", statusCode = 400 });
            }
            return response;
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgetPasswordDto forgetPasswordDto)
        {
            IActionResult response = Ok(new { message = "A One-Time Password (OTP) has been sent to your email." });
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                       .Select(e => e.ErrorMessage).ToList();
                response = BadRequest(new { message = "Invalid input. Please check the provided data and try again.", errors});
            }
            var email = forgetPasswordDto.Email;
            var opt = GenerateOtp();
            var existUser = await _userService.GetUserByEmailAsync(email);
            if (existUser == null){
                response = BadRequest(new { message = "No account found with this email address. Please check the email or register a new account."});
            }
            else {
                var IsValid = await _userService.UserForgetPassword(email, opt);
                if (!IsValid){
                    response = BadRequest(new { message = "An error occurred while processing your request. Please try again later."});
                }
            }
            if(response is OkObjectResult){
                await _notificationService.SendOtpEmail(existUser.email, opt);
            }
            return response;
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            IActionResult response = Ok(new { message = "Password has been reset successfully." });
            if (!ModelState.IsValid)
            {
                response = BadRequest(new { message = "Invalid input. Please check the provided data and try again.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
            }
            var hashPassword = HashPassword(dto.NewPassword);
            var isReset = await _userService.UserResetPassword(dto.Otp, hashPassword);
            if (!isReset)
                response = BadRequest(new { message = "The OTP is either invalid or has expired. Please request a new OTP.", statusCode = 400 });
            return response;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            IActionResult response = Ok(new {});
            if (!ModelState.IsValid)
            {
                response = BadRequest(new { message = "Invalid input. Please check the provided details and try again.", });
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
            IActionResult response = Ok(new { message = $"User with ID {userId} has been banned successfully." });
            var user = await _userService.GetUserByIdAsync(userId);
            if(user == null){
                response = BadRequest(new { message = "Unable to ban user. User not found."});
            }
            var isBanned = await _userService.BanUserAsync(userId);
            if(!isBanned){
                response = BadRequest(new { message = "An error occurred while attempting to ban the user."});
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

        private static User createUserObject(User user){
            user.password = HashPassword(user.password);
            user.verificationToken = GenerateVerfiedToken();
            user.verificationTokenExpiry = DateTime.UtcNow.AddHours(24);
            return user;
        }
    }
}
