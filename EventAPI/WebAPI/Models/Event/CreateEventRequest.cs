using System.ComponentModel.DataAnnotations;
using Application.Contracts.DTOs;
using WebAPI.Attributes;

namespace WebAPI.Models.Event;

/// <summary>
/// Представляет данные для создания события.
/// </summary>
/// <param name="Id">Идентификатор.</param>
/// <param name="Title">Наименование.</param>
/// <param name="Description">Описание.</param>
/// <param name="StartAt">Дата начала.</param>
/// <param name="EndAt">Дата окончания.</param>
public record CreateEventRequest(
	[Required] Guid Id,
	[Required(ErrorMessage = "Наименование события обязательно для заполнения.")]
	string Title,
	string? Description,
	[NotDefault(ErrorMessage = "Дата начала события обязательна для заполнения.")]
	DateTime StartAt,
	[NotDefault(ErrorMessage = "Дата окончания события обязательна для заполнения.")]
	DateTime EndAt,
	[NotDefault(ErrorMessage = "Общее количество мест на событии обязательно для заполнения.")]
	[Range(1, int.MaxValue, ErrorMessage = "Общее количество мест должно быть больше нуля.")]
	int TotalSeats)
	: EventRequest(Title, Description, StartAt, EndAt, TotalSeats)
{
	public EventDto ToDto()
	{
		return new EventDto(Id, Title, Description, StartAt, EndAt, TotalSeats);
	}
}