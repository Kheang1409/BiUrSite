using System.Security.Claims;
using Backend.Application.DTOs.Posts;
using Backend.Application.Posts.Create;
using Backend.Application.Posts.Delete;
using Backend.Application.Posts.Edit;
using Backend.Application.Posts.GetPost;
using Backend.Application.Posts.GetPosts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Backend.Api.Controllers;

[ApiController]
[Route("api/posts")]
public class PostController : ControllerBase
{
    private readonly IMediator _mediator;
    public PostController(
        IMediator mediator)
    {
        _mediator = mediator;
    }


    [HttpGet]
    public async Task<IActionResult> GetPosts([FromQuery] GetPostsQuery query)
    {
        var posts = await _mediator.Send(query);
        return Ok(new
        {
            success = true,
            data = posts.Select(post => (PostDto)post)
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPost(Guid id)
    {
        var query = new GetPostByIdQuery(id);
        var post = await _mediator.Send(query);
        return Ok(new
        {
            success = true,
            data = (PostDetailDto)post!
        });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Upload([FromBody] PostCreateDTOs dto)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
        var username = User.FindFirstValue(ClaimTypes.Name) ?? throw new UnauthorizedAccessException();
        var command = new CreatePostCommand(
            UserId: new Guid(ownerId),
            Username: username,
            Text: dto.Text,  
            Data: dto.Data);
        var post = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetPost), new { id = post.Id.Value }, new
        {
            success = true,
            message = "Post uploaded successfully"
        });
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] EditPostDto dto)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
        var command = new EditPostCommand(
            Id: id,
            UserId : new Guid(ownerId),
            Text: dto.Text
        );
        await _mediator.Send(command);
        return NoContent();
    } 

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
        var command = new DeletePostCommand(
            Id: id,
            UserId : new Guid(ownerId)
        );
        await _mediator.Send(command);
        return NoContent();
    }
}