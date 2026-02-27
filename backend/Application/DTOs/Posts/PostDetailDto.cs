using Backend.Application.DTOs.Comments;
using Backend.Domain.Enums;
using Backend.Domain.Posts;

namespace Backend.Application.DTOs.Posts;

public record PostDetailDto(
    string Id,
    string UserId,
    string Username,
    string UserProfile,
    string Text,
    string? ImageUrl,
    int CommentCount,
    DateTime CreatedDate
)
{
    public static explicit operator PostDetailDto(Post post)
    {
        return new PostDetailDto(
            post.Id.Value.ToString(),
            post.UserId.Value.ToString(),
            post.User!.Username,
            post.User!.Profile.Url,
            post.Text,
            post.Image?.Url,
            post.Comments.Count(c => c.Status == Status.Active),
            post.CreatedDate
        );
    }
}