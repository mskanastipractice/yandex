using Application.Contracts.DTOs;
using WebAPI.Models;

namespace WebAPI.Mappings;

internal static class EventMapping
{
	internal static PaginatedResult<EventResponse> ToPaginatedResponse(this PaginatedResultDto<EventDto> paginatedEvents) 
		=> new(paginatedEvents.ToResponse(), paginatedEvents.ToPageInfo());

	private static IReadOnlyCollection<EventResponse> ToResponse(this PaginatedResultDto<EventDto> paginatedEvents) 
		=> paginatedEvents
			.Items
			.Select(x => new EventResponse(x.Id, x.Title, x.Description, x.StartAt, x.EndAt))
			.ToArray();

	private static PageInfo ToPageInfo(this PaginatedResultDto<EventDto> paginatedEvents) 
		=> new(paginatedEvents.TotalItems, paginatedEvents.CurrentPage, paginatedEvents.ItemsPerPage);
}