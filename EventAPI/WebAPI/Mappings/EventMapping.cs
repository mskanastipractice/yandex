using Application.Contracts.DTOs;
using WebAPI.Models;
using WebAPI.Models.Event;

namespace WebAPI.Mappings;

internal static class EventMapping
{
	internal static PaginatedResult<EventResponse> ToPaginatedResponse(this PaginatedResultDto<EventInfoDto> paginatedEvents) 
		=> new(paginatedEvents.ToResponse(), paginatedEvents.ToPageInfo());

	private static IReadOnlyCollection<EventResponse> ToResponse(this PaginatedResultDto<EventInfoDto> paginatedEvents) 
		=> paginatedEvents
			.Items
			.Select(x => new EventResponse(x.Id, x.Title, x.Description, x.StartAt, x.EndAt, x.TotalSeats, x.AvailableSeats))
			.ToArray();

	private static PageInfo ToPageInfo(this PaginatedResultDto<EventInfoDto> paginatedEvents) 
		=> new(paginatedEvents.TotalItems, paginatedEvents.CurrentPage, paginatedEvents.ItemsPerPage);
}