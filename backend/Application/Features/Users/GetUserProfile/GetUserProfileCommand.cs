using Backend.Domain.Users.Entities;
using MediatR;

namespace Backend.Application.Features.Users.GetUserProfile;
public record GetUserProfileCommand(string Email) : IRequest<User?>;