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
    public static explicit operator CommentDto(Comment post)
    {
        return new CommentDto(
            post.Id.Value.ToString(),
            post.UserId.Value.ToString(),
            post.Username,
            post.UserProfile,
            post.Text,
            post.CreatedDate
        );
    }
}