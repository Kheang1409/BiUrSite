using Backend.Domain.Comments.Entities;
using MediatR;

namespace Backend.Application.Features.Comments.GetComments;
public record GetCommentsCommand(int PageNumber=1) : IRequest<List<Comment>>;