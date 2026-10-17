using Auth.Domain;
using Auth.Domain.User;
using MediatR;

namespace Auth.Application.Contracts.Queries;

public record GetUserByIdQuery(Guid UserId) : IRequest<User>;