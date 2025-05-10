using Backend.Domain.Posts.Entities;

namespace Backend.Application.DTOs;

public class PostDto
{
    public int Id { get; set; }
    public string Description { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedDate { get; set; }
    public UserDto Author { get; set; }

    public static PostDto FromPost(Post post)
    {
        if (post == null) 
            return null;
        return new PostDto
        {
            Id = post.PostId,
            Description = post.Description,
            IsDeleted = post.IsDeleted,
            CreatedDate = post.CreatedDate,
            Author = post.Author == null ? null : UserDto.FromUser(post.Author) ?? null
        };
    }
}