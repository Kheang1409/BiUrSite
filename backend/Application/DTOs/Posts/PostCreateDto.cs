namespace Backend.Application.DTOs.Posts;

public record PostCreateDTOs(
    string Text,
    byte[]? Data);