using Auth.Domain.User;
using MediatR;

namespace Auth.Application.Contracts.Commands;

public record RegisterUserCommand(string Login, string Password, UserRole Role) : IRequest;