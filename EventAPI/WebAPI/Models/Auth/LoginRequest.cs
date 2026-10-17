using Application.Contracts.DTOs;

namespace WebAPI.Models.Auth;

/// <summary>
/// Представляет данные для входа в систему.
/// </summary>
/// <param name="Login">Логин пользователя.</param>
/// <param name="Password">Пароль пользователя.</param>
public record LoginRequest(string Login, string Password) { 
    public LoginDto ToDto()
    {
        return new LoginDto(Login, Password);
    }
}