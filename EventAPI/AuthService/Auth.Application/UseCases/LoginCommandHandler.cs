using Auth.Application.Contracts.Commands;
using Auth.Application.Exceptions;
using Auth.Application.Interfaces;
using MediatR;

namespace Auth.Application.UseCases;

public class LoginCommandHandler(IPasswordHasher hasher, IJwtProvider jwtProvider, IUserRepository userRepository)
    : IRequestHandler<LoginCommand, string>
{
    public async Task<string> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindByLoginAsync(command.Login, cancellationToken);

        if (user is null)
        {
            throw new AuthenticationException();
        }

        return hasher.Verify(command.Password, user.PasswordHash)
            ? jwtProvider.GenerateToken(user)
            : throw new AuthenticationException();
    }
}