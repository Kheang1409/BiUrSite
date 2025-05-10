using Backend.Domain.Comments.Entities;
using MediatR;

namespace Backend.Application.Features.Comments.GetCommentById;
public record GetCommentByIdCommand(int CommentId) : IRequest<Comment?>;