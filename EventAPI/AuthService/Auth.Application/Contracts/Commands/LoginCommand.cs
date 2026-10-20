using MediatR;

namespace Auth.Application.Contracts.Commands;

public record LoginCommand(string Login, string Password) : IRequest<string>;