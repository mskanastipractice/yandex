using Auth.Api.Contracts.Users;
using Auth.Domain.User;
using UserRole = Auth.Api.Contracts.Users.UserRole;

namespace Auth.Api.Mapping;

internal static class UserMapping
{
    internal static UserResponse ToResponse(this User data)
    {
        return new UserResponse(data.Id, data.Login, MapRole(data.Role));
    }

    private static Auth.Domain.User.UserRole MapRole(UserRole value)
    {
        return value switch
        {
            UserRole.User => Domain.User.UserRole.User,
            UserRole.Admin => Domain.User.UserRole.Admin,
            _ => throw new ArgumentException($"Не найден маппинг для {value}")
        };
    }

    private static UserRole MapRole(Auth.Domain.User.UserRole value)
    {
        return value switch
        {
            Domain.User.UserRole.User => UserRole.User,
            Domain.User.UserRole.Admin => UserRole.Admin,
            _ => throw new ArgumentException($"Не найден маппинг для {value}")
        };
    }
}