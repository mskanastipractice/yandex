using Application.Contracts.DTOs;
using Application.Exceptions.Exceptions;

namespace Application.Contracts;

/// <summary>
/// Предоставляет интерфейс сервиса управления пользователями.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Возвращает пользователя по его идентификатору.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>DTO пользователя.</returns>
    /// <exception cref="EntityNotFoundException">Выбрасывается, если пользователь с указанным ID не найден.</exception>
    Task<UserDto> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
}
