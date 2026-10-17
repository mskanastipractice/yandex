using Application.Contracts;
using Application.Contracts.DTOs;
using Application.Exceptions.Exceptions;

namespace Application.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<UserDto> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException("Пользователь", userId);
        }
        return UserDto.ToDto(user);
    }
}