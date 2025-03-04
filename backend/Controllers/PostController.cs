using System.IdentityModel.Tokens.Jwt;
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
        public PostController(IPostService postService, IMapper mapper)
        {
            _postService = postService;
            _mapper= mapper;

        }

        [HttpGet]
        public async Task<IActionResult> GetPosts([FromQuery] int ? pageNumber=1, [FromQuery] string? keyword = null, int ? userId = null){
            IActionResult response = Ok(new {message = "Post data retrieved.", data = new List<object>() });
            if(pageNumber < 1)
                response = BadRequest(new { message = "Page number must be start from 1!"});
            if(response is OkObjectResult){
                int _pageNumber = (pageNumber ?? 1) - 1;
                var posts = await _postService.GetPostsAsync(_pageNumber, keyword, userId);
                response = Ok(new {message = "Post data retrieved.", data = posts});
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
                response = Ok(new {message = "Post data retrieved.", data = post});
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
            IActionResult response = Ok(new {message = $"Post with ID {postId} has been updated successfully.", data = new object() });
            var post = await _postService.GetPostByIdAsync(postId);
            if(post == null){
                response = BadRequest(new { message = " Post not found."});
            }
            
            if (post.userId != GetAuthId())
            {
                response = BadRequest(new { message = "You are not the owner of this post." });
            }
            if (response is OkObjectResult)
            {
                var isUpdated = await _postService.UpdateContentAsync(postId, postDto.description);
                if(!isUpdated){
                    response = BadRequest(new {message = "An error occurred while attempting to update the post.", data = post});
                }
            }
            
            return response;
        }

        [Authorize]
        [HttpDelete("{postId}")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            IActionResult response = Ok(new {message = $"Post with ID {postId} has been deleted successfully.", data = new object() });
            var post = await _postService.GetPostByIdAsync(postId);
            if(post == null){
                response = BadRequest(new { message = " Post not found."});
            }
            if (post.userId != GetAuthId())
            {
                response = BadRequest(new { message = "You are not the owner of this post." });
            }
            if (response is OkObjectResult)
            {
                var isDeleted = await _postService.DeletePostAsync(postId);
                if(!isDeleted){
                    response = BadRequest(new {message = "An error occurred while attempting to delete the post.", data = post});
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
