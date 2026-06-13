using Application.Contracts;
using Application.Contracts.DTOs;
using Application.Exceptions;
using Domain.Entities;
using Domain.Entities.ValueObjects;

namespace Application.Services;

public class EventService : IEventService
{
	private readonly List<Event> _events = [];
	
	public IEnumerable<EventDto> GetAll()
		=> _events.Select(EventDto.ToDto);

	public EventDto GetById(int eventId)
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

	public EventDto Update(int eventId, EventDto dto)
	{
		var eventToUpdate = _events.Find(e => e.Id == eventId);

		if (eventToUpdate is null)
		{
			throw new EntityNotFoundException("Событие", eventId);
		}

		eventToUpdate.Update(dto.Title, dto.Description, EventPeriod.Create(dto.StartAt, dto.EndAt));
		return EventDto.ToDto(eventToUpdate);
	}

	public void Delete(int eventId)
	{
		var eventToDelete = _events.Find(e => e.Id == eventId);

		if (eventToDelete is null)
		{
			throw new EntityNotFoundException("Событие", eventId);
		}

		_events.Remove(eventToDelete);
	}
}