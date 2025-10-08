using Backend.Domain.Posts;

namespace Backend.Application.DTOs.Posts;

public record PostDto(
    string Id,
    string UserId,
    string Username,
    string Text,
    string? ImageUrl,
    DateTime CreatedDate
)
{
    public static explicit operator PostDto(Post post)
    {
        return new PostDto(
            post.Id.Value.ToString(),
            post.UserId.Value.ToString(),
            post.Username,
            post.Text,
            post.Image?.Url,
            post.CreatedDate
        );
    }
}