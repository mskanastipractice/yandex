using Application.Contracts.DTOs;
using Application.Exceptions;

namespace Application.Contracts;

/// <summary>
/// Предоставляет интерфейс сервиса для управления событиями.
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Возвращает все доступные события.
    /// </summary>
    /// <returns>Коллекция событий.</returns>
    Task<PaginatedResultDto<EventInfoDto>> GetAllAsync(Filters filters, int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает событие по его идентификатору.
    /// </summary>
    /// <param name="eventId">Идентификатор события.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>DTO события.</returns>
    /// <exception cref="EntityNotFoundException">Выбрасывается, если событие с указанным ID не найдено.</exception>
    Task<EventInfoDto> GetByIdAsync(Guid eventId, CancellationToken cancellationToken);

    /// <summary>
    /// Создаёт новое событие.
    /// </summary>
    /// <param name="dto">Данные для создания события.</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>DTO созданного события.</returns>
    Task<EventInfoDto> CreateAsync(EventDto dto, CancellationToken cancellationToken);

    /// <summary>
    /// Обновляет существующее событие.
    /// </summary>
    /// <param name="eventId">Идентификатор обновляемого события.</param>
    /// <param name="dto">Новые данные события.</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>DTO обновлённого события.</returns>
    /// <exception cref="EntityNotFoundException">Выбрасывается, если событие с указанным ID не найдено.</exception>
    Task<EventInfoDto> UpdateAsync(Guid eventId, EventDto dto, CancellationToken cancellationToken);

    /// <summary>
    /// Удаляет событие по идентификатору.
    /// </summary>
    /// <param name="eventId">Идентификатор удаляемого события.</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <exception cref="EntityNotFoundException">Выбрасывается, если событие с указанным ID не найдено.</exception>
    Task DeleteAsync(Guid eventId, CancellationToken cancellationToken);

    /// <summary>
    /// Проверяет доступность бронирования мест на событие
    /// </summary>
    /// <param name="eventId">Идентификатор события.</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <param name="count">Количество мест.</param>
    Task<bool> TryReserveSeatsAsync(Guid eventId, CancellationToken cancellationToken, int count = 1);
}