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

        [HttpGet]
        public async Task<IActionResult> GetPosts([FromQuery] int? pageNumber = 1, [FromQuery] string? keyword = null, [FromQuery] int? userId = null)
        {
            if (pageNumber < 1)
                return BadRequest(new { message = "Page number must start from 1." });

            var cacheKey = $"posts_page_{pageNumber}_keyword_{keyword ?? "all"}_userId_{userId?.ToString() ?? "all"}";
            int _pageNumber = (pageNumber ?? 1) - 1;
            var posts = await _cache.GetDataAsync<List<Post>>(cacheKey) ?? await _postService.GetPostsAsync(_pageNumber, keyword, userId);

            return Ok(new { message = "Post data retrieved.", data = _mapper.Map<List<PostDto>>(posts) });
        }

        [Authorize]
        [HttpGet("total-post")]
        public async Task<IActionResult> GetUserTotalPosts()
        {
            var ownerId = GetAuthId();
            var postCount = await _postService.GetUserTotalPostAsync(ownerId);
            return Ok(new { postCount });
        }

        [HttpGet("{postId}")]
        public async Task<IActionResult> GetPost(int postId)
        {
            var post = await _postService.GetPostByIdAsync(postId);
            if (post == null)
                return NotFound(new { message = "Post not found." });

            return Ok(new { message = "Post data retrieved.", data = _mapper.Map<PostDto>(post) });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePost(CreatePostDto postDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { message = "Invalid input. Please check the provided data and try again.", errors });
            }

            var newPost = _mapper.Map<Post>(postDto);
            newPost.userId = GetAuthId();
            
            var post =  await _postService.AddPostAsync(newPost);
            if (post == null)
                return StatusCode(500, new { message = "An error occurred while attempting to create the post." });
            await _hubContext.Clients.All.SendAsync("ReceivePost", post);
            return StatusCode(201, new { message = "Post published successfully.", data = _mapper.Map<PostDto>(post)});
        }

        [Authorize]
        [HttpPut("{postId}")]
        public async Task<IActionResult> UpdatePost(int postId, CreatePostDto postDto)
        {
            var post = await _postService.GetPostByIdAsync(postId);
            if (post == null)
                return NotFound(new { message = "Post not found." });

            if (post.userId != GetAuthId())
                return Forbid();

            var isUpdated = await _postService.UpdateContentAsync(postId, postDto.description);
            if (!isUpdated)
                return StatusCode(500, new { message = "An error occurred while attempting to update the post." });

            return Ok(new { message = $"Post with ID {postId} has been updated successfully." });
        }

        [Authorize]
        [HttpDelete("{postId}")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var post = await _postService.GetPostByIdAsync(postId);
            if (post == null)
                return NotFound(new { message = "Post not found." });

            if (post.userId != GetAuthId())
                return Forbid();

            var isDeleted = await _postService.SoftDeletePostAsync(postId);
            if (!isDeleted)
                return StatusCode(500, new { message = "An error occurred while attempting to delete the post." });

            return NoContent();
        }

        [HttpGet("{postId}/comments")]
        public async Task<IActionResult> GetComments(int postId, [FromQuery] int? pageNumber = 1, string? keyword = null, int? userId = null)
        {
            var post = await _postService.GetPostByIdAsync(postId);
            if (post == null)
                return NotFound(new { message = "Post not found." });

            string cacheKey = $"comments_postId_{postId}_page_{pageNumber}_keyword_{keyword ?? "all"}_userId_{userId?.ToString() ?? "all"}";
            int _pageNumber = (pageNumber ?? 1) - 1;
            var comments = await _cache.GetDataAsync<List<Comment>>(cacheKey) ?? await _commentService.GetCommentsAsync(_pageNumber, keyword, userId, postId);

            return Ok(new { message = "Comment data retrieved.", data = _mapper.Map<List<CommentDto>>(comments) });
        }

        [HttpGet("{postId}/comments/{commentId}")]
        public async Task<IActionResult> GetComment(int postId, int commentId)
        {
            var comment = await _commentService.GetCommentByIdAsync(postId, commentId);
            if (comment == null)
                return NotFound(new { message = "Comment not found." });

            return Ok(new { message = "Comment data retrieved.", data = _mapper.Map<CommentDto>(comment) });
        }

        [Authorize]
        [HttpPost("{postId}/comments")]
        public async Task<IActionResult> CreateComment(int postId, CreateCommentDto commentDto)
        {
            var post = await _postService.GetPostByIdAsync(postId);
            if (post == null)
                return NotFound(new { message = "Post not found." });

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { message = "Invalid input. Please check the provided data and try again.", errors });
            }

            var comment = _mapper.Map<Comment>(commentDto);
            comment.postId = post.postId;
            comment.userId = GetAuthId();
            var createdComment = await _commentService.AddCommentAsync(comment);
            if (createdComment == null)
                return StatusCode(500, new { message = "An error occurred while attempting to create the comment." });

            var notification = GenerateNotification(post, comment);
            await _notificationService.AddNotificationAsync(notification);
            await _hubContext.Clients.User(post.userId.ToString()).SendAsync("ReceiveNotification", notification);
            return StatusCode(201, new { message = "Comment published successfully.", data = _mapper.Map<CommentDto>(createdComment) });
        }

        [Authorize]
        [HttpPut("{postId}/comments/{commentId}")]
        public async Task<IActionResult> UpdateComment(int postId, int commentId, CreateCommentDto commentDto)
        {
            var comment = await _commentService.GetCommentByIdAsync(postId, commentId);
            if (comment == null)
                return NotFound(new { message = "Comment not found." });

            if (comment.userId != GetAuthId())
                return Forbid();

            var isUpdated = await _commentService.UpdateContentAsync(postId, commentId, commentDto.description);
            if (!isUpdated)
                return StatusCode(500, new { message = "An error occurred while attempting to update the comment." });

            return Ok(new { message = $"Comment with ID {commentId} has been updated successfully." });
        }

        [Authorize]
        [HttpDelete("{postId}/comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(int postId, int commentId)
        {
            var comment = await _commentService.GetCommentByIdAsync(postId, commentId);
            if (comment == null)
                return NotFound(new { message = "Comment not found." });

            if (comment.userId != GetAuthId())
                return Forbid();

            var isDeleted = await _commentService.SoftDeleteCommentAsync(postId, commentId);
            if (!isDeleted)
                return StatusCode(500, new { message = "An error occurred while attempting to delete the comment." });

            return NoContent();
        }

        private int GetAuthId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userId ?? "0");
        }

        private string GetAuthUsername()
        {
            var username = User.FindFirst(ClaimTypes.GivenName)?.Value;
            return username ?? "";
        }

        private Notification GenerateNotification(Post post, Comment comment){
            var notification = new Notification
            {
                userId = post.author.userId,
                message = $"{GetAuthUsername()} comment on your post: {comment.description}",
                postId = post.postId,
                commentId = comment.commentId,
            };
            return notification;
        }
    }
}