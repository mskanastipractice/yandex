using Application.Contracts.DTOs;

namespace Application.Contracts;

/// <summary>
/// Предоставляет интерфейс сервиса для авторизации и аутентификации.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Выполняет регистрацию пользователя.
    /// </summary>
    /// <param name="registrationDto">Данные для регистрации\.</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <exception cref="UserAlreadyExistsException">Выбрасывается, если пользователь с такими данными уже существует.</exception>
    Task RegisterAsync(RegistrationDto registrationDto, CancellationToken cancellationToken);
    
    /// <summary>
    /// Выполняет вход пользователя.
    /// </summary>
    /// <param name="loginDto">Данных для входа.</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Возвращает токен пользователя</returns>
    /// <exception cref="AuthenticationException">Выбрасывается, если пользователь с такими данными не найден.</exception>
    Task<string> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken);
}