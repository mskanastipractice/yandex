using Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Mappings;
using WebAPI.Models;

namespace WebAPI.Controllers;

/// <summary>
/// API для управления событиями
/// </summary>
/// <param name="eventService">Сервис для работы с событиями</param>
[ApiController]
[Route("[controller]")]
public class EventsController(IEventService eventService) : ControllerBase
{
    /// <summary>
    /// Получить все события
    /// </summary>
    /// <returns>Список событий</returns>
    /// <response code="200">Успешно возвращен список событий</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<EventResponse>), StatusCodes.Status200OK)]
    public ActionResult<PaginatedResult<EventResponse>> GetAll([FromQuery] GetEventsQuery query)
    {
        var result = eventService.GetAll(new Filters(query.Title, query.From, query.To), query.Page, query.PageSize);
        return Ok(result.ToPaginatedResponse());
    }
    
    /// <summary>
    /// Получить событие по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор события (должен быть положительным числом)</param>
    /// <returns>Детальная информация о событии</returns>
    /// <response code="200">Событие найдено и успешно возвращено</response>
    /// <response code="404">Событие с указанным ID не найдено</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<EventResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById([FromRoute] int id) => Ok(eventService.GetById(id));

    /// <summary>
    /// Создать новое событие
    /// </summary>
    /// <param name="request">Данные для создания события</param>
    /// <returns>Созданное событие с присвоенным ID</returns>
    /// <response code="201">Событие успешно создано</response>
    /// <response code="400">Некорректные данные запроса (например, дата окончания раньше даты начала)</response>
    [HttpPost]
    [ProducesResponseType<EventResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CreateEventRequest request)
    {
        var result = eventService.Create(request.ToDto());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Обновить существующее событие
    /// </summary>
    /// <param name="id">ID события для обновления</param>
    /// <param name="request">Новые данные события</param>
    /// <response code="204">Событие успешно обновлено (тело ответа пустое)</response>
    /// <response code="400">Некорректные данные запроса</response>
    /// <response code="404">Событие с указанным ID не найдено</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType<EventResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(int id, [FromBody] EventRequest request) 
        => Ok(eventService.Update(id, request.ToDto(id)));

    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="id">ID события для удаления</param>
    /// <response code="200">Событие успешно удалено</response>
    /// <response code="404">Событие с указанным ID не найдено</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete([FromRoute] int id)
    {
        eventService.Delete(id);
        return Ok();
    }
}
