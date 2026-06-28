using Application.Contracts;
using Application.Contracts.DTOs;
using Application.Exceptions;
using Application.Extensions;
using Domain.Entities;
using Domain.Entities.ValueObjects;

namespace Application.Services;

public class EventService : IEventService
{
	private readonly List<Event> _events = [];
	
	public IEnumerable<EventDto> GetAll()
		=> _events.Select(EventDto.ToDto);

	public PaginatedResultDto<EventDto> GetAll(Filters filters, int page, int pageSize)
	{
		IEnumerable<Event> filteredEvents = _events
			.WhereIf(!string.IsNullOrWhiteSpace(filters.Title), x => x.Title.Contains(filters.Title!, StringComparison.OrdinalIgnoreCase))
			.WhereIf(filters.From.HasValue, x => x.Period.StartAt >= filters.From)
			.WhereIf(filters.To.HasValue, x => x.Period.EndAt <= filters.To);

		var totalItems = filteredEvents.Count();
		var result = filteredEvents.Skip((page - 1) * pageSize).Take(pageSize).Select(EventDto.ToDto).ToArray();

		return new PaginatedResultDto<EventDto>(totalItems, page, result.Length, result);
	}

	public EventDto GetById(Guid eventId)
	{
		var eventData = _events.Find(e => e.Id == eventId);
		return eventData != null ? EventDto.ToDto(eventData) : throw new EntityNotFoundException("Событие", eventId);
	}

	public EventDto Create(EventDto dto)
	{
		var eventData = Event.Create(dto.Id, dto.Title, dto.Description, EventPeriod.Create(dto.StartAt, dto.EndAt));
		_events.Add(eventData);
		
		return EventDto.ToDto(eventData) ;
	}

	public EventDto Update(Guid eventId, EventDto dto)
	{
		var eventToUpdate = _events.Find(e => e.Id == eventId);

		if (eventToUpdate is null)
		{
			throw new EntityNotFoundException("Событие", eventId);
		}

		eventToUpdate.Update(dto.Title, dto.Description, EventPeriod.Create(dto.StartAt, dto.EndAt));
		return EventDto.ToDto(eventToUpdate);
	}

	public void Delete(Guid eventId)
	{
		var eventToDelete = _events.Find(e => e.Id == eventId);

		if (eventToDelete is null)
		{
			throw new EntityNotFoundException("Событие", eventId);
		}

		_events.Remove(eventToDelete);
	}
}