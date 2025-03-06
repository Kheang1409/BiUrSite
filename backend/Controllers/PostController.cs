using System.Security.Claims;
using AutoMapper;
using Backend.DTOs;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        public PostController(IPostService postService, ICommentService commentService, IMapper mapper, ICacheService cache)
        {
            _postService = postService;
            _commentService = commentService;
            _mapper= mapper;
            _cache = cache;

        }

        [HttpGet]
        public async Task<IActionResult> GetPosts([FromQuery] int ? pageNumber=1, [FromQuery] string? keyword = null, [FromQuery] int ? userId = null){
            IActionResult response = Ok(new {message = "Post data retrieved.", data = new List<object>() });
            if(pageNumber < 1)
                response = BadRequest(new { message = "Page number must be start from 1!"});
            if(response is OkObjectResult){
                var cacheKey = $"posts_page_{pageNumber}_keyword_{keyword ?? "all"}_userId_{userId?.ToString() ?? "all"}";
                int _pageNumber = (pageNumber ?? 1) - 1;
                var posts = await _cache.GetDataAsync<List<Post>>(cacheKey) ?? await _postService.GetPostsAsync(_pageNumber, keyword, userId);
                response = Ok(new {message = "Post data retrieved.", data = _mapper.Map<List<PostDto>>(posts)});
            }
            return response;
        }
        [HttpGet("{postId}")]
        public async Task<IActionResult> GetPost(int postId){
            IActionResult response = Ok(new {message = "Post data retrieved.", data = new object() });
            var post = await _postService.GetPostByIdAsync(postId);
            if(post == null){
                response = BadRequest(new { message = "Post not found!"});
            }
            if(response is OkObjectResult){
                response = Ok(new {message = "Post data retrieved.", data = _mapper.Map<PostDto>(post)});
            }
            return response;
        }
        
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePost(CreatePostDto postDto)
        {
            IActionResult response = Ok(new { message = "Post published successfully.", data = new object()});
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                       .Select(e => e.ErrorMessage).ToList();
                response = BadRequest(new { message = "Invalid input. Please check the provided data and try again.", errors});
            }
            if(response is OkObjectResult)
            {
                var post = _mapper.Map<Post>(postDto);
                post.userId = GetAuthId();
                await _postService.AddPostAsync(post);
                response = Ok(new {message = "Post published successfully.", data = post});
            }
            return response;
        }

        [Authorize]
        [HttpPut("{postId}")]
        public async Task<IActionResult> UpdatePost(int postId, CreatePostDto postDto)
        {
            IActionResult response = Ok(new {message = $"Post with ID {postId} has been updated successfully."});
            var post = await _postService.GetPostByIdAsync(postId);
            if(post == null){
                response = BadRequest(new { message = " Post not found."});
            }
            else if (post.userId != GetAuthId())
            {
                response = BadRequest(new { message = "You are not the owner of this post." });
            }
            if (response is OkObjectResult)
            {
                var isUpdated = await _postService.UpdateContentAsync(postId, postDto.description);
                if(!isUpdated){
                    response = StatusCode(500, new {message = "An error occurred while attempting to update the post."});
                }
            }
            return response;
        }

        [Authorize]
        [HttpDelete("{postId}")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            IActionResult response = Ok(new {message = $"Post with ID {postId} has been deleted successfully."});
            var post = await _postService.GetPostByIdAsync(postId);
            if(post == null){
                response = BadRequest(new { message = " Post not found."});
            }
            else if(post.userId != GetAuthId())
            {
                response = BadRequest(new { message = "You are not the owner of this post." });
            }
            if (response is OkObjectResult)
            {
                var isDeleted = await _postService.SoftDeletePostAsync(postId);
                if(!isDeleted){
                    response = StatusCode(500, new {message = "An error occurred while attempting to delete the post."});
                }
            }
            return response;
        }

        [HttpGet("{postId}/comments")]
        public async Task<IActionResult> GetComments(int postId, [FromQuery] int ? pageNumber=1, string? keyword = null, int ? userId = null){
            IActionResult response = Ok(new {message = "Comment data retrieved.", data = new object() });
            var post = await _postService.GetPostByIdAsync(postId);
            if(post == null){
                response = BadRequest(new { message = "Post not found!"});
            }
            else{
                string cacheKey = $"comments_postId_{postId}_page_{pageNumber}_keyword_{keyword ?? "all"}_userId_{userId?.ToString() ?? "all"}";
                int _pageNumber = (pageNumber ?? 1) - 1;
                var comments =  await _cache.GetDataAsync<List<Comment>>(cacheKey) ?? await _commentService.GetCommentsAsync(_pageNumber, keyword, userId, postId);
                if(response is OkObjectResult){
                    response = Ok(new {message = "Comment data retrieved.", data = _mapper.Map<List<CommentDto>>(comments) });
                }
            }
            return response;
        }
        [HttpGet("{postId}/comments/{commentId}")]
        public async Task<IActionResult> GetComment(int postId, int commentId){
            IActionResult response = Ok(new {message = "Comment data retrieved.", data = new object() });
            var comment = await _commentService.GetCommentByIdAsync(postId, commentId);
            if(comment == null){
                response = BadRequest(new { message = "Comment not found!"});
            }
            if(response is OkObjectResult){
                response = Ok(new {message = "Comment data retrieved.", data = _mapper.Map<CommentDto>(comment)});
            }
            return response;
        }

        [Authorize]
        [HttpPost("{postId}/comments")]
        public async Task<IActionResult> CreateComment(int postId, CreateCommentDto commentDto){
            IActionResult response = Ok(new { message = "Comment published successfully.", data = new object()});
            var post = await _postService.GetPostByIdAsync(postId);
            if(post == null){
                response = BadRequest(new { message = "Post not found!"});
            }
            else{
                var comment = _mapper.Map<Comment>(commentDto);
                comment.postId = post.postId;
                comment.userId = GetAuthId();
                await _commentService.AddCommentAsync(comment);
                if(response is OkObjectResult){
                    response = Ok(new {message = "Comment published successfully.", data = comment});
                }
            }
            return response;
        }

        [Authorize]
        [HttpPut("{postId}/comments/{commentId}")]
        public async Task<IActionResult> UpdateComment(int postId, int commentId, CreateCommentDto commentDto){
            IActionResult response = Ok(new {message = $"Comment with ID {commentId} has been updated successfully."});
            var comment = await _commentService.GetCommentByIdAsync(postId, commentId);
            if(comment == null){
                response = BadRequest(new { message = "Comment not found."});
            }
            else if (comment.userId != GetAuthId())
            {
                response = BadRequest(new { message = "You are not the owner of this comment." });
            }
            if (response is OkObjectResult)
            {
                var isUpdated = await _commentService.UpdateContentAsync(postId, commentId, commentDto.description);
                if(!isUpdated){
                    response = StatusCode(500, new {message = "An error occurred while attempting to update the post."});
                }
            }
            return response;
        }

        [Authorize]
        [HttpDelete("{postId}/comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(int postId, int commentId){
            IActionResult response = Ok(new {message = $"Comment with ID {commentId} has been deleted successfully."});
            var comment = await _commentService.GetCommentByIdAsync(postId, commentId);
            if(comment == null){
                response = BadRequest(new { message = "Comment not found."});
            }
            else if (comment.userId != GetAuthId())
            {
                response = BadRequest(new { message = "You are not the owner of this comment." });
            }
            if (response is OkObjectResult)
            {
                var isUpdated = await _commentService.SoftDeleteCommentAsync(postId, commentId);
                if(!isUpdated){
                    response = StatusCode(500, new {message = "An error occurred while attempting to deleted the comment."});
                }
            }
            return response;
        }

        private int GetAuthId(){
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userId);
        }
    }
}
