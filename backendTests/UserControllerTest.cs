using Moq;
using Backend.Controllers;
using Backend.DTOs;
using Backend.Models;
using Backend.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BackendTest
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly UserController _controller;

        // Helper class to deserialize API responses
        private class ApiResponse
        {
            public string? message { get; set; }
            public object? data { get; set; }
            public List<string>? errors { get; set; }
        }

        public UserControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockJwtService = new Mock<IJwtService>();
            _mockEmailService = new Mock<IEmailService>();
            _mockCacheService = new Mock<ICacheService>();
            _mockMapper = new Mock<IMapper>();

            _controller = new UserController(
                _mockUserService.Object,
                _mockJwtService.Object,
                _mockEmailService.Object,
                _mockMapper.Object,
                _mockCacheService.Object
            );
        }

        [Fact]
        public async Task GetUsers_ReturnsOkResult_WithUsers()
        {
            // Arrange
            var users = new List<User> { new User { userId = 1, email = "test@example.com", password = "", profile = "", username = "" } };
            _mockCacheService.Setup(c => c.GetDataAsync<List<User>>("users_page_1_all")).ReturnsAsync(users);
            _mockMapper.Setup(m => m.Map<List<UserDto>>(users)).Returns(new List<UserDto> { new UserDto { email = "test@example.com", username = "" } });

            // Act
            var result = await _controller.GetUsers(page: 1, username: null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = JsonConvert.DeserializeObject<ApiResponse>(JsonConvert.SerializeObject(okResult.Value));
            Assert.Equal("Users retrieved successfully.", response?.message);
        }

        [Fact]
        public async Task GetUserById_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync((User?)null!);

            // Act
            var result = await _controller.GetUserById(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = JsonConvert.DeserializeObject<ApiResponse>(JsonConvert.SerializeObject(notFoundResult.Value));
            Assert.Equal("User not found.", response?.message);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenInvalidModelState()
        {
            // Arrange
            _controller.ModelState.AddModelError("email", "Email is required.");

            var registerDto = new RegisterDto
            {
                email = "test@example.com",
                password = "password123",
                profile = "",
                username = ""
            };

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = JsonConvert.DeserializeObject<ApiResponse>(JsonConvert.SerializeObject(badRequestResult.Value));
            Assert.Equal("Invalid input. Please check the provided data.", response?.message);
            Assert.NotNull(response?.errors);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenInvalidCredentials()
        {
            // Arrange
            var loginDto = new LoginDto { email = "test@example.com", password = "wrongpassword" };
            _mockUserService.Setup(s => s.GetUserByEmailAsync(loginDto.email)).ReturnsAsync((User?)null);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = JsonConvert.DeserializeObject<ApiResponse>(JsonConvert.SerializeObject(unauthorizedResult.Value));
            Assert.Equal("Invalid email or password.", response?.message);
        }

        [Fact]
        public async Task VerifyUser_ReturnsOkResult_WhenValidToken()
        {
            // Arrange
            var token = "validToken";
            _mockUserService.Setup(s => s.UserVerifiedAsync(token)).ReturnsAsync(true);

            // Act
            var result = await _controller.VerifyUser(token);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = JsonConvert.DeserializeObject<ApiResponse>(JsonConvert.SerializeObject(okResult.Value));
            Assert.Equal("Account successfully verified.", response?.message);
        }

        [Fact]
        public async Task ForgotPassword_ReturnsBadRequest_WhenInvalidModelState()
        {
            // Arrange
            _controller.ModelState.AddModelError("email", "Email is required.");
            var forgetPasswordDto = new ForgetPasswordDto { email = "test@example.com" };

            // Act
            var result = await _controller.ForgotPassword(forgetPasswordDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = JsonConvert.DeserializeObject<ApiResponse>(JsonConvert.SerializeObject(badRequestResult.Value));
            Assert.Equal("Invalid input. Please check the provided data.", response?.message);
            Assert.NotNull(response?.errors);
        }

        [Fact]
        public async Task ResetPassword_ReturnsOkResult_WhenValidOtp()
        {
            // Arrange
            var resetPasswordDto = new ResetPasswordDto { otp = "123456", newPassword = "newPassword123" };
            _mockUserService.Setup(s => s.UserResetPasswordAsync(resetPasswordDto.otp, It.IsAny<string>())).ReturnsAsync(true);

            // Act
            var result = await _controller.ResetPassword(resetPasswordDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = JsonConvert.DeserializeObject<ApiResponse>(JsonConvert.SerializeObject(okResult.Value));
            Assert.Equal("Password reset successfully.", response?.message);
        }

        [Fact]
        public async Task BanUser_ReturnsOkResult_WhenSuccessful()
        {
            // Arrange
            var user = new User { userId = 1, email = "test@example.com", password = "", profile = "", username = "" };
            _mockUserService.Setup(s => s.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(user);
            _mockUserService.Setup(s => s.BanUserAsync(It.IsAny<int>())).ReturnsAsync(true);

            // Act
            var result = await _controller.BanUser(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = JsonConvert.DeserializeObject<ApiResponse>(JsonConvert.SerializeObject(okResult.Value));
            Assert.Equal($"User with ID 1 has been banned.", response?.message);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var user = new User { userId = 1, email = "test@example.com", password = "", profile = "", username = "" };
            _mockUserService.Setup(s => s.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(user);
            _mockUserService.Setup(s => s.SoftDeleteUserAsync(It.IsAny<int>())).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteUser(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}