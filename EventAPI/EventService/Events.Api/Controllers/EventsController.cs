using Events.Api.Contracts;
using Events.Api.Contracts.Events;
using Events.Api.Mappings;
using Events.Application.Contracts.Commands;
using Events.Application.Contracts.Queries;
using Events.Application.QueryHandlers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Events.Api.Controllers;

/// <summary>
/// Представляет контроллер для событий.
/// </summary>
[Authorize]
[ApiController]
[Route("[controller]")]
public class EventsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Получить все события
    /// </summary>
    /// <returns>Список событий</returns>
    /// <response code="200">Успешно возвращен список событий</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<EventResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<EventResponse>>> GetAll([FromQuery] GetEventsQuery data, CancellationToken cancellationToken)
    {
        var result = await sender.Send(data.ToQuery(), cancellationToken);

        return Ok(result.ToPaginatedResponse());
    }
    
    [AllowAnonymous]
    [HttpGet("top")]
    [ProducesResponseType(typeof(IReadOnlyList<EventResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EventResponse>>> GetTop(CancellationToken cancellationToken)
    {
        var events = await sender.Send(new GetTopEventsQuery(), cancellationToken);
        return Ok(events.Select(x => x.ToResponse()).ToArray());
    }

    /// <summary>
    /// Получить событие по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор события (должен быть положительным числом)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Детальная информация о событии</returns>
    /// <response code="200">Событие найдено и успешно возвращено</response>
    /// <response code="404">Событие с указанным ID не найдено</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        return Ok((await sender.Send(new GetEventByIdQuery(id), cancellationToken)).ToResponse());
    }
    
    /// <summary>
    /// Создать новое событие
    /// </summary>
    /// <param name="request">Данные для создания события</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Созданное событие с присвоенным ID</returns>
    /// <response code="201">Событие успешно создано</response>
    /// <response code="400">Некорректные данные запроса (например, дата окончания раньше даты начала)</response>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EventResponse>> Create([FromBody] EventRequest request, CancellationToken cancellationToken)
    {
        var eventId = await sender.Send(request.ToAddCommand(), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = eventId }, eventId);
    }
    
    /// <summary>
    /// Обновить существующее событие
    /// </summary>
    /// <param name="id">ID события для обновления</param>
    /// <param name="request">Новые данные события</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <response code="204">Событие успешно обновлено (тело ответа пустое)</response>
    /// <response code="400">Некорректные данные запроса</response>
    /// <response code="404">Событие с указанным ID не найдено</response>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] EventRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(request.ToUpdateCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Удалить событие
    /// </summary>
    /// <param name="id">ID события для удаления</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <response code="200">Событие успешно удалено</response>
    /// <response code="404">Событие с указанным ID не найдено</response>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteEventCommand(id), cancellationToken);
        return Ok();
    }
}