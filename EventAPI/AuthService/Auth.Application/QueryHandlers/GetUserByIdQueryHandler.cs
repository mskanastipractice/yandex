using Auth.Application.Contracts.Queries;
using Auth.Application.Exceptions;
using Auth.Application.Interfaces;
using Auth.Domain.User;
using MediatR;

namespace Auth.Application.QueryHandlers;

public class GetUserByIdQueryHandler(IUserRepository userRepository) : IRequestHandler<GetUserByIdQuery, User>
{
    public async Task<User> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindAsync(query.UserId, cancellationToken);

        return user ?? throw new EntityNotFoundException("Пользователь", query.UserId);
    }
}