using Application.Contracts.DTOs;
using WebAPI.Models.Users;

namespace WebAPI.Models.Auth;

/// <summary>
/// Представляет данные для регистрации пользователя в системе.
/// </summary>
/// <param name="Login">Логин пользователя.</param>
/// <param name="Password">Пароль пользователя.</param>
/// <param name="Role">Роль пользователя.</param>
public record RegistrationRequest(string Login, string Password, UserRole Role = UserRole.User){
    public RegistrationDto ToDto()
    {
        return new RegistrationDto(Login, Password, MapRole(Role));
    }
    
    private static Domain.Enums.UserRole MapRole(UserRole value)
    {
        return value switch
        {
            UserRole.User => Domain.Enums.UserRole.User,
            UserRole.Admin => Domain.Enums.UserRole.Admin,
            _ => throw new ArgumentException($"Не найден маппинг для {value}")
        };
    }
}