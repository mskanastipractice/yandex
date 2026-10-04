using Application.Contracts.DTOs;
using Domain.Entities;
using WebAPI.Models;
using WebAPI.Models.Users;

namespace WebAPI.Mappings;

public static class UserMapping
{
    internal static UserResponse ToResponse(this UserDto data)
    {
        return new UserResponse(data.Id, data.Login, MapRole(data.Role));
    }

    private static UserRole MapRole(Domain.Enums.UserRole value)
    {
        return value switch
        {
            Domain.Enums.UserRole.User =>UserRole.User,
            Domain.Enums. UserRole.Admin => UserRole.Admin,
            _ => throw new ArgumentException($"Не найден маппинг для {value}")
        };
    }
}