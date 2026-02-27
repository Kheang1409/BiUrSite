namespace Backend.Application.Users.VerifyUser;

using MediatR;

public record VerifyUserCommand(string Token) : IRequest;