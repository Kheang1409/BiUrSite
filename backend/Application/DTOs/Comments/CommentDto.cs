using Backend.Domain.Comments;

namespace Backend.Application.DTOs.Comments;

public record CommentDto(
    string Id,
    string UserId,
    string Username,
    string UserProfile,
    string Text,
    DateTime CreatedDate
)
{
    public static explicit operator CommentDto(Comment comment)
    {
        return new CommentDto(
            comment.Id.Value.ToString(),
            comment.UserId.Value.ToString(),
            comment.Username,
            comment.UserProfile,
            comment.Text,
            comment.CreatedDate
        );
    }
}