using Auth.Application.Contracts.Commands;
using Auth.Application.Exceptions;
using Auth.Application.Interfaces;
using Auth.Domain.User;
using MediatR;

namespace Auth.Application.UseCases;

public class RegisterUserCommandHandler(IPasswordHasher hasher, IUserRepository userRepository)
    : IRequestHandler<RegisterUserCommand>
{
    public async Task Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var passwordHash = hasher.Hash(command.Password);
        var userId = Guid.NewGuid();

        if (await userRepository.ExistsByLoginAsync(command.Login, cancellationToken))
        {
            throw new UserAlreadyExistsException();
        }

        await userRepository.AddAsync(User.Create(userId, command.Login, passwordHash, command.Role));
        await userRepository.SaveChangesAsync(cancellationToken);
    }
}