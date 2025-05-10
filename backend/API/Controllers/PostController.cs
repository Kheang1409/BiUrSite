using System.Security.Claims;
using Backend.Api.Hubs;
using Backend.Application.Features.Posts.GetPosts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MediatR;
using Backend.Application.Features.Posts.CountUserTotalPost;
using Backend.Application.Features.Posts.GetPostById;
using Backend.Application.Features.Posts.CreatePost;
using Backend.Application.Features.Posts.UpdatePost;
using Backend.Application.Features.Posts.DeletePost;
using Backend.Application.Features.Comments.GetComments;
using Backend.Application.Features.Comments.GetCommentById;
using Backend.Application.Features.Comments.CreateComment;
using Backend.Application.Features.Users.GetUserProfile;
using Backend.Application.Features.Notifications.CreateNotification;
using Backend.Application.Features.Comments.UpdateComment;
using Backend.Application.Features.Comments.DeleteComment;
using Backend.Application.DTOs;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/posts")]
    public class PostController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IMediator _mediator;

        public PostController(
            IHubContext<NotificationHub> hubContext,
            IMediator mediator)
        {
            _hubContext = hubContext;
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetPosts([FromQuery] GetPostsCommand command)
        {
            try
            {
                if (command == null) 
                    return BadRequest(new { message = "Invalid request." });
                var posts = await _mediator.Send(command);
                return Ok(posts.Select(PostDto.FromPost));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching posts.", error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("total")]
        public async Task<IActionResult> GetUserTotalPosts()
        {
            try{
                var postCount = await _mediator.Send(new CountUserTotalPostCommand(GetAuthId()));
                return Ok(new { postCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching user post count.", error = ex.Message });
            }
        }

        [HttpGet("{postId}")]
        public async Task<IActionResult> GetPost(int postId)
        {
            try{
                var post = await _mediator.Send(new GetPostByIdCommand(postId));
                if (post == null) 
                    return NotFound(new { message = "Post not found." });
                return Ok(PostDto.FromPost(post));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the post.", error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostCommand command)
        {
            try
            {
                if (command == null) 
                    return BadRequest(new { message = "Invalid request." });
                var post = await _mediator.Send(new CreatePostWithUserIdCommand(command.Description, GetAuthId()));
                await _hubContext.Clients.All.SendAsync("ReceivePost", PostDto.FromPost(post));
                return CreatedAtAction(nameof(GetPost), new { postId = post.PostId }, PostDto.FromPost(post));
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the post.", error = ex.Message });
            }
            
        }

        [Authorize]
        [HttpPut("{postId}")]
        public async Task<IActionResult> UpdatePost(int postId, [FromBody] UpdatePostCommand command)
        {
            if (command == null) return BadRequest("Invalid request.");
            try
            {
                var post = await _mediator.Send(new GetPostByIdCommand(postId));
                if(post == null) 
                    return NotFound(new { message = "Post not found." });
                if (post.UserId != GetAuthId())
                    return Forbid();
                await _mediator.Send(new UpdatePostWithPostIdCommand(postId, command.Description));
                return NoContent();
                
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the post.", error = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("{postId}")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            try
            {
                var post = await _mediator.Send(new GetPostByIdCommand(postId));
                if (post == null) 
                    return NotFound(new { message = "Post not found." });
                if (post.UserId != GetAuthId())
                        return Forbid();
                await _mediator.Send(new DeletePostCommand(postId));

                return NoContent(); // 204 No Content
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the post.", error = ex.Message });
            }
            
        }

        [HttpGet("{postId}/comments")]
        public async Task<IActionResult> GetComments(int postId, [FromQuery] GetCommentsCommand command)
        {
            try
            {
                var post = await _mediator.Send(new GetPostByIdCommand(postId));
                if(post== null)
                    return NotFound(new { message = "Post not found." }); 
                var comments = await _mediator.Send(new GetCommentsWithPostIdCommand(postId, command.PageNumber));
                return Ok(comments.Select(CommentDto.FromComment));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the comments.", error = ex.Message });
            }
        }

        [HttpGet("{postId}/comments/{commentId}")]
        public async Task<IActionResult> GetComment(int postId, int commentId)
        {
            try
            {
                var post = await _mediator.Send( new GetPostByIdCommand(postId));
                if(post == null)
                    return NotFound(new { message = "Post not found." }); 
                var comment = await _mediator.Send(new GetCommentByIdCommand(commentId));
                if(comment == null)
                    return NotFound(new { message = "Comment not found." }); 
                return Ok(CommentDto.FromComment(comment));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the comment.", error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("{postId}/comments")]
        public async Task<IActionResult> CreateComment(int postId, [FromBody] CreateCommentCommand command)
        {
            try{
                if (command == null) 
                    return BadRequest("Invalid request.");
                var authEmail = GetAuthEmail();
                var user = await _mediator.Send(new GetUserProfileCommand(authEmail));
                if (user == null) 
                    return NotFound(new { message = "User not found." });
                var post = await _mediator.Send(new GetPostByIdCommand(postId));
                if (post == null)
                    return NotFound(new { message = "Post not found." });
                var comment = await _mediator.Send(new CreateCommentWithIdsCommand(postId, user.Id, command.Description));
                var message = $"{post.Author?.Username} commented on your post: {comment.Description}";
                var notification = await _mediator.Send(new CreateNotificationCommand(message, post.Author.Id, postId, comment.Id));
                if (comment == null) 
                    return StatusCode(500, new { message = "An error occurred while creating the comment." });

                if (post.UserId != user.Id)
                {
                    await _hubContext.Clients.User(post.UserId.ToString()).SendAsync("ReceiveNotification", notification);
                }
                return CreatedAtAction(nameof(GetComment), new { postId, commentId = comment.Id }, comment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the comment.", error = ex.Message });
            }
            
        }

        [Authorize]
        [HttpPut("{postId}/comments/{commentId}")]
        public async Task<IActionResult> UpdateComment(int postId, int commentId, [FromBody] UpdateCommentCommand command)
        {
            try{
                if (command == null) 
                    return BadRequest("Invalid request.");
                var authEmail = GetAuthEmail();
                var user = await _mediator.Send(new GetUserProfileCommand(authEmail));
                if (user == null) 
                    return NotFound(new { message = "User not found." });
                var post = await _mediator.Send(new GetPostByIdCommand(postId));
                if (post == null)
                    return NotFound(new { message = "Post not found." });
                var comment = await _mediator.Send(new GetCommentByIdCommand(commentId));
                if (comment == null) 
                    return NotFound(new { message = "Comment not found." });
                if (comment.UserId != user.Id) 
                    return Forbid(); 
                var isUpdated = await _mediator.Send(new UpdateCommentWithIdCommand(commentId, command.Description));
                if (!isUpdated) 
                    return StatusCode(500, new { message = "An error occurred while updating the comment." });

                return NoContent();
            }
            catch(Exception ex){
                 return StatusCode(500, new { message = "An error occurred while updating the comment.", error = ex.Message });
            }
            
        }

        [Authorize]
        [HttpDelete("{postId}/comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(int postId, int commentId)
        {
            try
            {
                var authEmail = GetAuthEmail();
                var user = await _mediator.Send(new GetUserProfileCommand(authEmail));
                if (user == null) 
                    return NotFound(new { message = "User not found." });
                var post = await _mediator.Send(new GetPostByIdCommand(postId));
                if (post == null)
                    return NotFound(new { message = "Post not found." });
                var comment = await _mediator.Send(new GetCommentByIdCommand(commentId));
                if (comment == null) 
                    return NotFound(new { message = "Comment not found." });
                if (comment.UserId != user.Id) 
                    return Forbid(); 
                var isDeleted = await _mediator.Send(new DeleteCommentCommand(commentId));
                if (!isDeleted) 
                    return StatusCode(500, new { message = "An error occurred while delete the comment." });
                return NoContent();
            }
            catch(Exception ex){
                 return StatusCode(500, new { message = "An error occurred while deleting the comment.", error = ex.Message });
            }
        }

        private int GetAuthId() 
            => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) ? userId : 0;

        private string GetAuthEmail() 
            => User.FindFirst(ClaimTypes.Email)?.Value ?? "";
    }
}