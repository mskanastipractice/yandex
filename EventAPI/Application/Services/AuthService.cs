using Application.Contracts;
using Application.Contracts.DTOs;
using Application.Exceptions.Exceptions;
using Domain.Entities;

namespace Application.Services;

public class AuthService(IPasswordHasher hasher, IJwtProvider jwtProvider, IUserRepository userRepository) : IAuthService
{
    public async Task RegisterAsync(RegistrationDto registrationDto, CancellationToken cancellationToken)
    {
        var passwordHash = hasher.Hash(registrationDto.Password);
        if (await userRepository.ExistsByLoginAsync(registrationDto.Login, cancellationToken))
        {
            throw new UserAlreadyExistsException();
        }

        var user = User.Create(registrationDto.Login, passwordHash, registrationDto.Role);
        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindByLoginAsync(loginDto.Login, cancellationToken);

        if (user is null)
        {
            throw new AuthenticationException();
        }

        return hasher.Verify(loginDto.Password, user.PasswordHash)
            ? jwtProvider.GenerateToken(user)
            : throw new AuthenticationException();
    }
}