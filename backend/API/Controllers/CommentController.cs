using System.Security.Claims;
using Backend.API.Helpers;
using Backend.Application.Comments.Create;
using Backend.Application.Comments.Delete;
using Backend.Application.Comments.Edit;
using Backend.Application.Comments.GetComment;
using Backend.Application.Comments.GetComments;
using Backend.Application.DTOs.Comments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Backend.Api.Controllers;

[ApiController]
[Route("api/posts/{postId:guid}/comments")]
public class CommentController : ControllerBase
{
    private readonly IMediator _mediator;
    public CommentController(
        IMediator mediator)
    {
        _mediator = mediator;
    }


    [HttpGet]
    public async Task<IActionResult> GetComments(Guid postId, [FromQuery] int pageNumber = 1)
    {
        var query = new GetCommentsQuery(
            PostId: postId,
            PageNumber: pageNumber
        );
        var comments = await _mediator.Send(query);
        return Ok(new
        {
            success = true,
            data = comments.Select(c =>(CommentDto)c)
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetComment(Guid postId, Guid id)
    {
        var query = new GetCommentByIdQuery(
            PostId: postId,
            Id: id);
        var comment = await _mediator.Send(query);
        return Ok(new
        {
            success = true,
            data = (CommentDto)comment!
        });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Comment(Guid postId, [FromBody] CommentCreateDTO dto)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
        var command = new CreateCommentCommand(
            PostId: postId,
            UserId: new Guid(ownerId),
            Text: dto.Text
        );
        var comment = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetComment), new { postId = postId, id = comment.Id.Value }, new
        {
            success = true,
            data = (CommentDto)comment!,
            message = "Comment uploaded successfully"
        });
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid postId, Guid id, [FromBody] EditCommentDto dto)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
        var command = new EditCommentCommand(
            Id: id,
            PostId: postId,
            UserId : new Guid(ownerId),
            Text: dto.Text
        );
        await _mediator.Send(command);
        return NoContent();
    } 

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid postId, Guid id)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
        var command = new DeleteCommentCommand(
            PostId: postId,
            Id: id,
            UserId : new Guid(ownerId)
        );
        await _mediator.Send(command);
        return NoContent();
    }
}