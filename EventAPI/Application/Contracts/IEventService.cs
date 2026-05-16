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
    IEnumerable<EventDto> GetAll();
    
    /// <summary>
    /// Возвращает событие по его идентификатору.
    /// </summary>
    /// <param name="eventId">Идентификатор события.</param>
    /// <returns>DTO события.</returns>
    /// <exception cref="EntityNotFoundException">Выбрасывается, если событие с указанным ID не найдено.</exception>
    EventDto GetById(int eventId);
    
    /// <summary>
    /// Создаёт новое событие.
    /// </summary>
    /// <param name="dto">Данные для создания события.</param>
    /// <returns>DTO созданного события.</returns>
    EventDto Create(EventDto dto);
    
    /// <summary>
    /// Обновляет существующее событие.
    /// </summary>
    /// <param name="eventId">Идентификатор обновляемого события.</param>
    /// <param name="dto">Новые данные события.</param>
    /// <returns>DTO обновлённого события.</returns>
    /// <exception cref="EntityNotFoundException">Выбрасывается, если событие с указанным ID не найдено.</exception>
    EventDto Update(int eventId, EventDto dto);
    
    /// <summary>
    /// Удаляет событие по идентификатору.
    /// </summary>
    /// <param name="eventId">Идентификатор удаляемого события.</param>
    /// <exception cref="EntityNotFoundException">Выбрасывается, если событие с указанным ID не найдено.</exception>
    void Delete(int eventId);
}