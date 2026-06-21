using System.ComponentModel.DataAnnotations;
using Application.Contracts.DTOs;
using WebAPI.Attributes;

namespace WebAPI.Models.Event;

/// <summary>
/// Представляет данные для создания или изменения события.
/// </summary>
/// <param name="Title">Наименование события. Обязательное поле.</param>
/// <param name="Description">Подробное описание события. Необязательное поле.</param>
/// <param name="StartAt">Дата и время начала события. Не может быть значением по умолчанию.</param>
/// <param name="EndAt">Дата и время окончания события. Не может быть значением по умолчанию.</param>
public record EventRequest(
	[Required(ErrorMessage = "Наименование события обязательно для заполнения.")]
	string Title,
	string? Description,
	[NotDefault(ErrorMessage = "Дата начала события обязательна для заполнения.")]
	DateTime StartAt,
	[NotDefault(ErrorMessage = "Дата окончания события обязательна для заполнения.")]
	DateTime EndAt)
{
	/// <summary>
	/// Преобразует запрос в DTO события для передачи в сервисный слой.
	/// </summary>
	/// <param name="eventId">Уникальный идентификатор события.</param>
	/// <returns>Объект EventDto, готовый для передачи в бизнес-логику.</returns>
	public EventDto ToDto(Guid eventId)
	{
		return new EventDto(eventId, Title, Description, StartAt, EndAt);
	}
}