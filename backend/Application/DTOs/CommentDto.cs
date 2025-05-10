using Backend.Domain.Comments.Entities;

namespace Backend.Application.DTOs;

public class CommentDto
{
    public int Id { get; set; }
    public string Description { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedDate { get; set; }
    public UserDto Commenter { get; set; }
    public static CommentDto FromComment(Comment comment)
    {
        if (comment == null) 
            return null;
        return new CommentDto
        {
            Id = comment.Id,
            Description = comment.Description,
            IsDeleted = comment.IsDeleted,
            CreatedDate = comment.CreatedDate,
            Commenter = comment.Commenter != null ? UserDto.FromUser(comment.Commenter) : null
        };
    }
}