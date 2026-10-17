using Domain.Entities;
using Domain.Enums;

namespace Application.Contracts.DTOs;

public record UserDto(Guid Id, string Login, UserRole Role)
{
    public static UserDto ToDto(User entity) => new(
        entity.Id,
        entity.Login,
        entity.Role
    );
}