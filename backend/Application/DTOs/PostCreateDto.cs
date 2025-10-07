namespace Backend.Application.DTOs;

public record PostCreateDTOs(
    string Text,
    byte[]? Data);