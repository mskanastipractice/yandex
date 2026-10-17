using Auth.Api.Contracts.Auth;
using Auth.Api.Contracts.Users;
using Auth.Application.Contracts.Commands;

namespace Auth.Api.Mapping;

internal static class AuthMapping
{
    internal static RegisterUserCommand ToCommand(this RegistrationRequest dto)
    {
        return new RegisterUserCommand(dto.Login, dto.Password, MapRole(dto.Role));
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
}