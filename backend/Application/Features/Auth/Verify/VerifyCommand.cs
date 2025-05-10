using MediatR;

namespace Backend.Application.Features.Auth.Verify;

public record VerifyCommand(string Token) : IRequest<bool>;