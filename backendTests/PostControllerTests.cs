using Moq;
using Backend.Controllers;
using Backend.Services;
using Backend.DTOs;
using Backend.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http;
using Backend.Hubs;
using System.Security.Claims;

namespace Backend.Tests
{
    public class PostControllerTests
    {
        private readonly Mock<IPostService> _mockPostService;
        private readonly Mock<ICommentService> _mockCommentService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IHubContext<NotificationHub>> _mockHubContext;
        private readonly PostController _controller;

        public PostControllerTests()
        {
            _mockPostService = new Mock<IPostService>();
            _mockCommentService = new Mock<ICommentService>();
            _mockMapper = new Mock<IMapper>();
            _mockCacheService = new Mock<ICacheService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockHubContext = new Mock<IHubContext<NotificationHub>>();

            _controller = new PostController(
                _mockPostService.Object,
                _mockCommentService.Object,
                _mockMapper.Object,
                _mockCacheService.Object,
                _mockNotificationService.Object,
                _mockHubContext.Object
            );
        }

        [Fact]
        public async Task GetPost_ReturnsOkResult_WhenPostExists()
        {
            // Arrange
            var post = new Post { postId = 1, description = "Test post" };
            var postDto = new PostDto { postId = 1, description = "Test post" };
            
            _mockPostService.Setup(service => service.GetPostByIdAsync(1)).ReturnsAsync(post);
            _mockMapper.Setup(mapper => mapper.Map<PostDto>(post)).Returns(postDto);

            // Act
            var result = await _controller.GetPost(1);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);
            var returnedPostDto = Assert.IsType<PostDto>(actionResult.Value);
            Assert.Equal("Test post", returnedPostDto.description);
        }

        [Fact]
        public async Task GetPosts_ReturnsPaginatedPosts_WhenPageAndKeywordProvided()
        {
            // Arrange
            var mockPosts = new List<Post> { new Post { postId = 1, description = "Test post" } };
            var mockPostDtos = new List<PostDto> { new PostDto { postId = 1, description = "Test post" } };

            _mockCacheService.Setup(c => c.GetDataAsync<List<Post>>(It.IsAny<string>())).ReturnsAsync((List<Post>?)null);
            _mockPostService.Setup(s => s.GetPostsAsync(0, "test")).ReturnsAsync(mockPosts);
            _mockMapper.Setup(m => m.Map<List<PostDto>>(mockPosts)).Returns(mockPostDtos);

            // Act
            var result = await _controller.GetPosts(page: 1, keyword: "test");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            var returnedPosts = Assert.IsType<List<PostDto>>(okResult.Value);
            Assert.Single(returnedPosts);
        }

        [Fact]
        public async Task GetPosts_ReturnsBadRequest_WhenPageIsLessThanOne()
        {
            // Act
            var result = await _controller.GetPosts(page: 0);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task GetPost_ReturnsNotFound_WhenPostDoesNotExist()
        {
            // Arrange
            _mockPostService.Setup(service => service.GetPostByIdAsync(1)).ReturnsAsync((Post?)null);

            // Act
            var result = await _controller.GetPost(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            
            // Instead of using dynamic, we'll work with the anonymous type properly
            var value = notFoundResult.Value as dynamic; // This is still dynamic, but we'll handle it differently
            var message = value?.GetType().GetProperty("message")?.GetValue(value, null) as string;
            
            Assert.Equal("Post not found.", message);
        }

        [Fact]
        public async Task GetUserTotalPosts_ReturnsPostCount_WhenUserAuthenticated()
        {
            // Arrange
            var mockUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] 
            { 
                new Claim(ClaimTypes.NameIdentifier, "1") 
            }, "TestAuthentication"));
            
            _controller.ControllerContext = new ControllerContext 
            { 
                HttpContext = new DefaultHttpContext { User = mockUser } 
            };

            _mockPostService.Setup(s => s.GetUserTotalPostAsync(1)).ReturnsAsync(5);

            // Act
            var result = await _controller.GetUserTotalPosts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            
            // Null-safe checking
            Assert.NotNull(okResult.Value);
            var responseType = okResult.Value.GetType();
            var postCountProperty = responseType.GetProperty("postCount");
            Assert.NotNull(postCountProperty);
            
            var postCountValue = postCountProperty.GetValue(okResult.Value);
            Assert.NotNull(postCountValue);
            Assert.Equal(5, (int)postCountValue!); // Using null-forgiving operator here since we've asserted it's not null
        }

        [Fact]
        public async Task CreatePost_ReturnsCreatedPost_WhenValidRequest()
        {
            // Arrange
            var mockUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = mockUser } };

            var newPost = new Post { postId = 1, description = "New post", userId = 1 };
            var newPostDto = new CreatePostDto { description = "New post" };
            var expectedPostDto = new PostDto { postId = 1, description = "New post" };

            // Mock SignalR HubContext
            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);
            _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);

            _mockMapper.Setup(m => m.Map<Post>(newPostDto)).Returns(newPost);
            _mockPostService.Setup(s => s.AddPostAsync(newPost)).ReturnsAsync(newPost);
            _mockMapper.Setup(m => m.Map<PostDto>(newPost)).Returns(expectedPostDto);

            // Act
            var result = await _controller.CreatePost(newPostDto);

            // Assert
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdAtResult.StatusCode);
            Assert.Equal(expectedPostDto, createdAtResult.Value);
        }

        [Fact]
        public async Task CreatePost_ReturnsBadRequest_WhenModelStateInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("description", "Required");

            // Act
            var result = await _controller.CreatePost(new CreatePostDto{description= ""});

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task UpdatePost_ReturnsNoContent_WhenAuthorized()
        {
            // Arrange
            var mockUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = mockUser } };

            var existingPost = new Post { postId = 1, description = "Old post", userId = 1 };
            var updateDto = new CreatePostDto { description = "Updated post" };

            _mockPostService.Setup(s => s.GetPostByIdAsync(1)).ReturnsAsync(existingPost);
            _mockPostService.Setup(s => s.UpdateContentAsync(1, "Updated post")).ReturnsAsync(true);

            // Act
            var result = await _controller.UpdatePost(1, updateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdatePost_ReturnsForbid_WhenUnauthorized()
        {
            // Arrange
            var mockUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "2") })); // Different user
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = mockUser } };

            var existingPost = new Post { postId = 1, description = "Old post", userId = 1 }; // Owned by user 1

            _mockPostService.Setup(s => s.GetPostByIdAsync(1)).ReturnsAsync(existingPost);

            // Act
            var result = await _controller.UpdatePost(1, new CreatePostDto{description=""});

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task DeletePost_ReturnsNoContent_WhenAuthorized()
        {
            // Arrange
            var mockUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = mockUser } };

            var existingPost = new Post { postId = 1, userId = 1, description="hi!" };
            _mockPostService.Setup(s => s.GetPostByIdAsync(1)).ReturnsAsync(existingPost);
            _mockPostService.Setup(s => s.SoftDeletePostAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeletePost(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeletePost_ReturnsNotFound_WhenPostMissing()
        {
            // Arrange
            _mockPostService.Setup(s => s.GetPostByIdAsync(1)).ReturnsAsync((Post?)null);

            // Act
            var result = await _controller.DeletePost(1);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CreateComment_SendsNotification_WhenNotPostOwner()
        {
            // Arrange
            var mockUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { 
                new Claim(ClaimTypes.NameIdentifier, "2"), 
                new Claim(ClaimTypes.GivenName, "Commenter") 
            }));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = mockUser } };

            var post = new Post { 
                postId = 1, 
                userId = 1, 
                description = "Hi!", 
                author = new User { userId = 1, email = "example@gmail.com", username = "admin", profile="", password="" } 
            };
            var commentDto = new CreateCommentDto { description = "Nice post!" };
            var newComment = new Comment { commentId = 1, description = "Nice post!", postId = 1, userId = 2 };
            var mockNotification = new Notification{userId=1, message="admin says Nice post!"};

            _mockPostService.Setup(s => s.GetPostByIdAsync(1)).ReturnsAsync(post);
            _mockMapper.Setup(m => m.Map<Comment>(commentDto)).Returns(newComment);
            _mockCommentService.Setup(s => s.AddCommentAsync(newComment)).ReturnsAsync(newComment);
            _mockNotificationService.Setup(s => s.AddNotificationAsync(It.IsAny<Notification>()))
                .ReturnsAsync(mockNotification); // Return Task<Notification>

            // Mock SignalR HubContext
            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            mockClients.Setup(clients => clients.User(It.IsAny<string>())).Returns(mockClientProxy.Object);
            _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);

            // Act
            var result = await _controller.CreateComment(1, commentDto);

            // Assert
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdAtResult.StatusCode);
            _mockNotificationService.Verify(s => s.AddNotificationAsync(It.IsAny<Notification>()), Times.Once);
        }
    }
}