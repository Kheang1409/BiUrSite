using System.Security.Claims;
using AutoMapper;
using Backend.DTOs;
using Backend.Hubs;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/posts")]
    public class PostController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        private readonly ICacheService _cache;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public PostController(
            IPostService postService, 
            ICommentService commentService, 
            IMapper mapper, 
            ICacheService cache,
            INotificationService notificationService,
            IHubContext<NotificationHub> hubContext)
        {
            _postService = postService;
            _commentService = commentService;
            _mapper = mapper;
            _cache = cache;
            _notificationService = notificationService;
            _hubContext = hubContext;
        }

        // Fetch posts (supports pagination, filters)
        [HttpGet]
        public async Task<IActionResult> GetPosts([FromQuery] int? page = 1, [FromQuery] string? keyword = null)
        {
            if (page < 1) 
                return BadRequest(new { message = "Page number must start from 1." });

            var cacheKey = $"posts_page_{page}_keyword_{keyword ?? "all"}";
            var posts = await _cache.GetDataAsync<List<Post>>(cacheKey) ?? await _postService.GetPostsAsync(page.Value - 1, keyword);

            return Ok(_mapper.Map<List<PostDto>>(posts)); // 200 OK
        }

        // Fetch total posts by authenticated user
        [Authorize]
        [HttpGet("total")]
        public async Task<IActionResult> GetUserTotalPosts()
        {
            var ownerId = GetAuthId();
            var postCount = await _postService.GetUserTotalPostAsync(ownerId);
            return Ok(new { postCount }); // 200 OK
        }

        // Fetch single post by ID
        [HttpGet("{postId}")]
        public async Task<IActionResult> GetPost(int postId)
        {
            var post = await _postService.GetPostByIdAsync(postId);
            if (post == null) 
                return NotFound(new { message = "Post not found." }); // 404 Not Found

            return Ok(_mapper.Map<PostDto>(post)); // 200 OK
        }

        // Create a new post (authenticated users)
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto postDto)
        {
            if (!ModelState.IsValid) 
                return BadRequest(new { message = "Invalid input. Please check the provided data.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() }); // 400 Bad Request

            var newPost = _mapper.Map<Post>(postDto);
            newPost.userId = GetAuthId();
            var createdPost = await _postService.AddPostAsync(newPost);
            if (createdPost == null) 
                return StatusCode(500, new { message = "An error occurred while creating the post." }); // 500 Internal Server Error

            await _hubContext.Clients.All.SendAsync("ReceivePost", createdPost);
            return CreatedAtAction(nameof(GetPost), new { postId = createdPost.postId }, _mapper.Map<PostDto>(createdPost)); // 201 Created
        }

        // Update post by ID (authenticated user only)
        [Authorize]
        [HttpPut("{postId}")]
        public async Task<IActionResult> UpdatePost(int postId, [FromBody] CreatePostDto postDto)
        {
            var post = await _postService.GetPostByIdAsync(postId);
            if (post == null) 
                return NotFound(new { message = "Post not found." }); // 404 Not Found

            if (post.userId != GetAuthId()) 
                return Forbid(); // 403 Forbidden

            var isUpdated = await _postService.UpdateContentAsync(postId, postDto.description);
            if (!isUpdated) 
                return StatusCode(500, new { message = "An error occurred while updating the post." }); // 500 Internal Server Error

            return NoContent(); // 204 No Content
        }

        // Delete post by ID (authenticated user only)
        [Authorize]
        [HttpDelete("{postId}")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var post = await _postService.GetPostByIdAsync(postId);
            if (post == null) 
                return NotFound(new { message = "Post not found." }); // 404 Not Found

            if (post.userId != GetAuthId()) 
                return Forbid(); // 403 Forbidden

            var isDeleted = await _postService.SoftDeletePostAsync(postId);
            if (!isDeleted) 
                return StatusCode(500, new { message = "An error occurred while deleting the post." }); // 500 Internal Server Error

            return NoContent(); // 204 No Content
        }

        // Fetch comments for a specific post (pagination, filters)
        [HttpGet("{postId}/comments")]
        public async Task<IActionResult> GetComments(int postId, [FromQuery] int? page = 1, [FromQuery] string? keyword = null)
        {
            var post = await _postService.GetPostByIdAsync(postId);
            if (post == null) 
                return NotFound(new { message = "Post not found." }); // 404 Not Found

            string cacheKey = $"comments_postId_{postId}_page_{page}_keyword_{keyword ?? "all"}";
            var comments = await _cache.GetDataAsync<List<Comment>>(cacheKey) ?? await _commentService.GetCommentsAsync(page.Value - 1, keyword, postId);

            return Ok(_mapper.Map<List<CommentDto>>(comments)); // 200 OK
        }

        // Fetch a specific comment by ID
        [HttpGet("{postId}/comments/{commentId}")]
        public async Task<IActionResult> GetComment(int postId, int commentId)
        {
            var comment = await _commentService.GetCommentByIdAsync(postId, commentId);
            if (comment == null) 
                return NotFound(new { message = "Comment not found." }); // 404 Not Found

            return Ok(_mapper.Map<CommentDto>(comment)); // 200 OK
        }

        // Create a new comment (authenticated users)
        [Authorize]
        [HttpPost("{postId}/comments")]
        public async Task<IActionResult> CreateComment(int postId, [FromBody] CreateCommentDto commentDto)
        {
            var post = await _postService.GetPostByIdAsync(postId);
            if (post == null) 
                return NotFound(new { message = "Post not found." }); // 404 Not Found

            if (!ModelState.IsValid) 
                return BadRequest(new { message = "Invalid input. Please check the provided data.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() }); // 400 Bad Request

            var comment = _mapper.Map<Comment>(commentDto);
            comment.postId = post.postId;
            comment.userId = GetAuthId();
            var createdComment = await _commentService.AddCommentAsync(comment);

            if (createdComment == null) 
                return StatusCode(500, new { message = "An error occurred while creating the comment." }); // 500 Internal Server Error

            if (post.author?.userId != comment.userId)
            {
                var notification = GenerateNotification(post, createdComment);
                await Task.WhenAll(
                    _notificationService.AddNotificationAsync(notification),
                    _hubContext.Clients.User(post?.author?.userId.ToString()).SendAsync("ReceiveNotification", notification)
                );
            }

            return CreatedAtAction(nameof(GetComment), new { postId, commentId = createdComment.commentId }, _mapper.Map<CommentDto>(createdComment)); // 201 Created
        }

        // Update comment by ID (authenticated user only)
        [Authorize]
        [HttpPut("{postId}/comments/{commentId}")]
        public async Task<IActionResult> UpdateComment(int postId, int commentId, [FromBody] CreateCommentDto commentDto)
        {
            var comment = await _commentService.GetCommentByIdAsync(postId, commentId);
            if (comment == null) 
                return NotFound(new { message = "Comment not found." }); // 404 Not Found

            if (comment.userId != GetAuthId()) 
                return Forbid(); // 403 Forbidden

            var isUpdated = await _commentService.UpdateContentAsync(postId, commentId, commentDto.description);
            if (!isUpdated) 
                return StatusCode(500, new { message = "An error occurred while updating the comment." }); // 500 Internal Server Error

            return NoContent(); // 204 No Content
        }

        // Delete comment by ID (authenticated user only)
        [Authorize]
        [HttpDelete("{postId}/comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(int postId, int commentId)
        {
            var comment = await _commentService.GetCommentByIdAsync(postId, commentId);
            if (comment == null) 
                return NotFound(new { message = "Comment not found." }); // 404 Not Found

            if (comment.userId != GetAuthId()) 
                return Forbid(); // 403 Forbidden

            var isDeleted = await _commentService.SoftDeleteCommentAsync(postId, commentId);
            if (!isDeleted) 
                return StatusCode(500, new { message = "An error occurred while deleting the comment." }); // 500 Internal Server Error

            return NoContent(); // 204 No Content
        }

        private int GetAuthId() 
            => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        private string GetAuthUsername() 
            => User.FindFirst(ClaimTypes.GivenName)?.Value ?? "";

        private Notification GenerateNotification(Post post, Comment comment) 
            => new Notification 
            {
                userId = post.author?.userId ?? 0,
                message = $"{GetAuthUsername()} commented on your post: {comment.description}",
                postId = post.postId,
                commentId = comment.commentId,
            };
    }
}