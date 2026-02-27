using Backend.Domain.Posts;

namespace Backend.Application.DTOs.Posts;

public record PostDto(
    string Id,
    string UserId,
    string Username,
    string UserProfile,
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
            post.User!.Username,
            post.User!.Profile.Url,
            post.Text,
            post.Image?.Url,
            post.CreatedDate
        );
    }
}